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

        protected override int Run()
        {
            bool flg = false;
            int cnt = (from t in Framework.tester
                       where t.HuntList.Count < 3 && t.RecList.Count < 3
                       select t).Count();
            using (var db = new CHDB())
            {
                foreach (var r in (from r in db.RECORDs
                                   where r.Status == (int)Record.StatusType.Pending
                                   select r).Take(cnt).ToArray())
                {
                    flg = true;
                    r.Status = (int)Record.StatusType.Running;
                    db.SaveChanges();
                    Framework.tester[rand.Next(Framework.tester.Count)].RecList.Add(r.ID);
                }
                foreach (var h in (from h in db.HUNTs
                                   where h.Status == (int)Hunt.StatusType.Pending
                                   select h).Take(cnt).ToArray())
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