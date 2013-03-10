using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Concurrent;
using ContestHunter.Database;

namespace ContestHunter.Models.Domain
{
    public class DispatcherDaemon : Daemon
    {
        Random rand = new Random();

        void HuntBadWating(CHDB db)
        {
            if (!(from t in Framework.tester
                  where t.HuntList.Count != 0
                  select t).Any())
            {
                foreach (var h in (from h in db.HUNTs
                                   where h.Status == (int)Hunt.StatusType.Running
                                   select h))
                    h.Status = (int)Hunt.StatusType.Pending;
            }
            db.SaveChanges();
        }

        void RecordBadWating(CHDB db)
        {
            if (!(from t in Framework.tester
                  where t.RecList.Count != 0
                  select t).Any())
            {
                foreach (var h in (from h in db.RECORDs
                                   where h.Status == (int)Record.StatusType.Running
                                   select h))
                    h.Status = (int)Record.StatusType.Pending;
            }
            db.SaveChanges();
        }

        protected override int Run()
        {
            bool flg = false;
            using (var db = new CHDB())
            {
                HuntBadWating(db);
                RecordBadWating(db);
                foreach (var r in (from r in db.RECORDs
                                    where r.Status == (int)Record.StatusType.Pending
                                    select r).ToArray())
                {
                    flg = true;
                    r.Status = (int)Record.StatusType.Running;
                    db.SaveChanges();
                    Framework.tester[rand.Next(Framework.tester.Count)].RecList.Add(r.ID);
                }
                foreach (var h in (from h in db.HUNTs
                                    where h.Status == (int)Hunt.StatusType.Pending
                                    select h).ToArray())
                {
                    flg = true;
                    h.Status = (int)Record.StatusType.Running;
                    db.SaveChanges();
                    Framework.tester[rand.Next(Framework.tester.Count)].HuntList.Add(h.ID);
                }
            }
            if (flg)
                return 0;
            return 3000;
        }
    }
}