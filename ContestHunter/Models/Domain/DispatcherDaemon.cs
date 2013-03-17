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
            var testers = (from t in Framework.tester
                           where t.HuntList.Count < 3 && t.RecList.Count < 3
                           select t).ToArray();
            for (int i = 0; i < testers.Length; i++)
            {
                var x = testers[i];
                int p = rand.Next(testers.Length);
                testers[i] = testers[p];
                testers[p] = x;
            }
            using (var db = new CHDB())
            {
                foreach (var tester in testers)
                {
                    var rec = (from r in db.RECORDs
                               where r.Status == (int)Record.StatusType.Pending
                               select r).FirstOrDefault();
                    if (null != rec)
                    {
                        flg = true;
                        rec.Status = (int)Record.StatusType.Running;
                        db.SaveChanges();
                        tester.RecList.Add(rec.ID);
                    }
                    var hu = (from h in db.HUNTs
                              where h.Status == (int)Hunt.StatusType.Pending
                              select h).FirstOrDefault();
                    if (null != hu)
                    {
                        flg = true;
                        hu.Status = (int)Hunt.StatusType.Running;
                        db.SaveChanges();
                        tester.HuntList.Add(hu.ID);
                    }
                }
            }
            if (flg)
                return 0;
            return 3000;
        }
    }
}