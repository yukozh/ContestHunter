using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContestHunter.Database;

namespace ContestHunter.Models.Domain
{
    public class ContestDaemon : Daemon
    {
        protected override int Run()
        {
            using (var db = new CHDB())
            {
                var con = (from c in db.CONTESTs
                           where c.EndTime < DateTime.Now && c.Status!=(int)Contest.StatusType.Done
                           select c).FirstOrDefault();
                if (null == con)
                    return 300000;
                switch ((Contest.ContestType)con.Type)
                {
                    case Contest.ContestType.ACM:
                        if (con.Status == (int)Contest.StatusType.BeforeFinalTest)
                        {
                            con.Status = (int)Contest.StatusType.FinalTesting;
                        }
                        else
                        {
                            if (!(from r in db.RECORDs
                                  where r.PROBLEM1.CONTEST1 == con &&
                                  r.SubmitTime <= con.EndTime &&
                                  r.Status == (int)Record.StatusType.Pending
                                  select r).Any())
                                con.Status = (int)Contest.StatusType.Done;
                        }
                        break;
                    case Contest.ContestType.CF:
                        if (con.Status == (int)Contest.StatusType.BeforeFinalTest)
                        {
                            foreach (TESTDATA test in (from t in db.TESTDATAs
                                                       where t.PROBLEM1.CONTEST1 == con && t.Available == false
                                                       select t))
                            {
                                test.Available = true;
                            }
                            foreach (RECORD rec in (from r in db.RECORDs
                                                    where r.PROBLEM1.CONTEST1 == con
                                                    select r))
                            {
                                rec.Status = (int)Record.StatusType.Pending;
                            }
                            con.Status = (int)Contest.StatusType.FinalTesting;
                        }
                        else
                        {
                            if (!(from r in db.RECORDs
                                  where r.PROBLEM1.CONTEST1 == con &&
                                  r.SubmitTime <= con.EndTime &&
                                  r.Status == (int)Record.StatusType.Pending
                                  select r).Any())
                                con.Status = (int)Contest.StatusType.Done;
                        }
                        break;
                    case Contest.ContestType.OI:
                        if (con.Status == (int)Contest.StatusType.BeforeFinalTest)
                        {
                            con.Status = (int)Contest.StatusType.FinalTesting;
                        }
                        else
                        {
                            if (!(from r in db.RECORDs
                                  where r.PROBLEM1.CONTEST1 == con &&
                                  r.SubmitTime <= con.EndTime &&
                                  r.Status == (int)Record.StatusType.Pending
                                  select r).Any())
                                con.Status = (int)Contest.StatusType.Done;
                        }
                        break;
                }
                db.SaveChanges();
            }
            return 300000;
        }
    }
}