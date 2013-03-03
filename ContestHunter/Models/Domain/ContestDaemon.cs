using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContestHunter.Database;

namespace ContestHunter.Models.Domain
{
    public class ContestDaemon : Daemon
    {
        void CalcRating(CONTEST con,CHDB db)
        {
            var contest = Contest.ByName(con.Name);
            string[] Rank;
            int[] Rating;
            double[] exp;
            switch (contest.Type)
            {
                case Contest.ContestType.OI:
                    Rank = contest.GetOIStanding(0, contest.AttendedUsersCount(), false, false).Select(x => x.User).ToArray();
                    break;
                case Contest.ContestType.CF:
                    Rank = contest.GetCFStanding(0, contest.AttendedUsersCount(), false, false).Select(x => x.User).ToArray();
                    break;
                case Contest.ContestType.ACM:
                    Rank = contest.GetACMStanding(0, contest.AttendedUsersCount(), false, false).Select(x => x.User).ToArray();
                    break;
                default:
                    throw new NotImplementedException();
            }
            Rating = (from u in Rank
                      let Rat = (from r in db.RATINGs
                                 where r.USER1.Name == u
                                 orderby r.CONTEST1.EndTime descending
                                 select r.Rating1).FirstOrDefault()
                      select Rat == 0 ? 1500 : Rat).ToArray();
            int n = Rank.Length;
            int m = n / 2 + 1;
            exp = new double[n];
            double mid = (double)Rating.Sum() / n;
            for (int i = 1; i <= n / 2; i++)
                exp[i] = mid + (3000 - mid) / Math.Pow(m - 1, 3) * Math.Pow(m - i, 3);
            exp[n / 2 + 1] = mid;
            for(int i=n/2+2;i<=n;i++)
                exp[i] = mid - mid / Math.Pow(n-m,3) * Math.Pow(i-m,3);
            int weight = con.Weight;
            for (int i = 1; i <= n; i++)
            {
                double k = (double)weight / m * Math.Abs(i - m) + 1;
                Rating[i] += (int)Math.Round((exp[i] - Rating[i]) * Math.Pow(n, 1.0 / 8.0) / k);
                Rating[i] = Math.Max(Rating[i], 1);
                Rating[i] = Math.Min(Rating[i], 3000);
            }
            for (int i = 1; i <= n; i++)
            {
                var name = Rank[i];
                foreach (var u in (from u in db.USERs
                                   where u.Name == name
                                   select u))
                {
                    u.RATINGs.Add(new RATING()
                    {
                        CONTEST1 = con,
                        Rating1 = Rating[i]
                    });
                }
            }
            
        }

        protected override int Run()
        {
            using (var db = new CHDB())
            {
                var con = (from c in db.CONTESTs
                           where c.EndTime+TimeSpan.FromMinutes(30) < DateTime.Now && c.Status!=(int)Contest.StatusType.Done
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
                                  where r.PROBLEM1.CONTEST1.ID == con.ID &&
                                  r.SubmitTime <= con.EndTime &&
                                  r.Status == (int)Record.StatusType.Pending
                                  select r).Any())
                            {
                                if (con.IsOfficial)
                                    CalcRating(con, db);
                                con.Status = (int)Contest.StatusType.Done;
                            }
                        }
                        break;
                    case Contest.ContestType.CF:
                        if (con.Status == (int)Contest.StatusType.BeforeFinalTest)
                        {
                            foreach (TESTDATA test in (from t in db.TESTDATAs
                                                       where t.PROBLEM1.CONTEST1.ID == con.ID && t.Available == false
                                                       select t))
                            {
                                test.Available = true;
                            }
                            foreach (RECORD rec in (from r in db.RECORDs
                                                    where r.PROBLEM1.CONTEST1.ID == con.ID
                                                    select r))
                            {
                                rec.Status = (int)Record.StatusType.Pending;
                            }
                            con.Status = (int)Contest.StatusType.FinalTesting;
                        }
                        else
                        {
                            if (!(from r in db.RECORDs
                                  where r.PROBLEM1.CONTEST1.ID == con.ID &&
                                  r.SubmitTime <= con.EndTime &&
                                  r.Status == (int)Record.StatusType.Pending
                                  select r).Any())
                            {
                                if (con.IsOfficial)
                                    CalcRating(con, db);
                                con.Status = (int)Contest.StatusType.Done;
                            }
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
                                  where r.PROBLEM1.CONTEST1.ID == con.ID &&
                                  r.SubmitTime <= con.EndTime &&
                                  r.Status == (int)Record.StatusType.Pending
                                  select r).Any())
                            {
                                if (con.IsOfficial)
                                    CalcRating(con, db);
                                con.Status = (int)Contest.StatusType.Done;
                            }
                        }
                        break;
                }
                db.SaveChanges();
            }
            return 300000;
        }
    }
}