using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContestHunter.Database;

namespace ContestHunter.Models.Domain
{
    public class ContestDaemon : Daemon
    {
        public static Dictionary<string, Guid> HuntLst = new Dictionary<string, Guid>();
        void CalcRating(CONTEST con, CHDB db)
        {
            List<string> Rank = new List<string>();
            List<int> Rating = new List<int>();
            double[] exp;
            int probs;
            int[] dat1;
            int[] dat2;

            switch ((Contest.ContestType)con.Type)
            {
                case Contest.ContestType.OI:
                    var tmp_oi = db.GetOIStanding(con.ID, DateTime.Now, 0, con.CONTEST_ATTEND.Count, false, false).ToArray();
                    dat1 = new int[con.CONTEST_ATTEND.Count + 100];
                    dat2 = new int[con.CONTEST_ATTEND.Count + 100];
                    probs = con.PROBLEMs.Count;
                    for (int i = 0; i < tmp_oi.Length; i += probs)
                    {
                        Rank.Add(tmp_oi[i].User);
                        int k = i / probs;
                        dat1[k] = dat2[k] = 0;
                        for (int j = i; j < i + probs; j++)
                        {
                            dat1[k] += tmp_oi[j].Score ?? 0;
                            dat2[k] += tmp_oi[j].ExecuteTime ?? 0;
                        }
                    }
                    break;
                case Contest.ContestType.CF:
                    var tmp_cf = db.GetCFStanding(con.ID, DateTime.Now, 0, con.CONTEST_ATTEND.Count, false, false).ToArray();
                    dat1 = new int[con.CONTEST_ATTEND.Count + 100];
                    dat2 = new int[con.CONTEST_ATTEND.Count + 100];
                    probs = con.PROBLEMs.Count;
                    for (int i = 0; i < tmp_cf.Length; i += probs)
                    {
                        Rank.Add(tmp_cf[i].User);
                        int k = i / probs;
                        dat1[k] = dat2[k] = 0;
                        for (int j = i; j < i + probs; j++)
                        {
                            dat1[k] += (tmp_cf[j].Rating ?? 0) + (tmp_cf[j].SuccessfulHunt ?? 0) * 100 - (tmp_cf[j].FailedHunt ?? 0) * 25;
                        }
                    }
                    break;
                case Contest.ContestType.ACM:
                    var tmp_acm = db.GetACMStanding(con.ID, DateTime.Now, 0, con.CONTEST_ATTEND.Count, false, false).ToArray();
                    dat1 = new int[con.CONTEST_ATTEND.Count + 100];
                    dat2 = new int[con.CONTEST_ATTEND.Count + 100];
                    probs = con.PROBLEMs.Count;
                    for (int i = 0; i < tmp_acm.Length; i += probs)
                    {
                        Rank.Add(tmp_acm[i].User);
                        int k = i / probs;
                        dat1[k] = dat2[k] = 0;
                        for (int j = i; j < i + probs; j++)
                        {
                            dat1[k] += (tmp_acm[j].IsAC ?? false) ? 1 : 0;
                            dat2[k] += (tmp_acm[j].ACTime ?? 0) + (tmp_acm[j].FailedTimes ?? 0) * 20;
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
            for (int i = 0, j; i < con.CONTEST_ATTEND.Count; i=j)
            {
                for (j = i + 1; j < con.CONTEST_ATTEND.Count && dat1[j] == dat1[i] && dat2[j] == dat2[i]; j++) ;
                for (int k = i; k < j; k++) dat1[k] = (i + j - 1) / 2;
            }
            foreach (var u in Rank)
            {
                var Rat = (from r in db.RATINGs
                           where r.USER1.Name == u
                           orderby r.CONTEST1.EndTime descending
                           select r.Rating1).FirstOrDefault();
                Rating.Add(Rat == 0 ? 1500 : Rat);
            }
            int n = Rank.Count;
            if (n < 1)
                return;
            int m = (n + 1) / 2;
            exp = new double[n];
            double mid = (double)Rating.Sum() / n;
            for (int i = 0; i < n / 2; i++)
                exp[i] = mid + (3000 - mid) / Math.Pow(m - 1, 3) * Math.Pow(m - i - 1, 3);
            exp[n / 2] = mid;
            for (int i = n / 2 + 1; i < n; i++)
                exp[i] = mid - mid / Math.Pow(n - m, 3) * Math.Pow(i + 1 - m, 3);
            int weight = con.Weight;
            for (int i = 0; i < n; i++)
            {
                Rating[i] += (int)Math.Round((exp[dat1[i]] - Rating[i]) * Math.Pow(n, 1.0 / 8.0) / weight);
                Rating[i] = Math.Max(Rating[i], 1);
                Rating[i] = Math.Min(Rating[i], 3000);
            }
            for (int i = 0; i < n; i++)
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
                           where c.EndTime < DateTime.Now && c.Status != (int)Contest.StatusType.Done
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
                                  (r.Status == (int)Record.StatusType.Pending || r.Status == (int)Record.StatusType.Running)
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
                            if ((from h in db.HUNTs
                                 where h.RECORD1.PROBLEM1.CONTEST1.ID == con.ID
                                 && (h.Status == (int)Hunt.StatusType.Pending || h.Status == (int)Hunt.StatusType.Running)
                                 select h).Any())
                                break;
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
                            string[] probs = (from p in con.PROBLEMs
                                              select p.ID.ToString()).ToArray();
                            lock (HuntLst)
                            {
                                foreach (string key in HuntLst.Keys.ToArray())
                                {
                                    bool flg = false;
                                    foreach (string prob in probs)
                                        if (key.EndsWith(prob))
                                        {
                                            flg = true;
                                            break;
                                        }
                                    if (flg)
                                    {
                                        HuntLst.Remove(key);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!(from r in db.RECORDs
                                  where r.PROBLEM1.CONTEST1.ID == con.ID &&
                                  r.SubmitTime <= con.EndTime &&
                                  (r.Status == (int)Record.StatusType.Pending || r.Status == (int)Record.StatusType.Running)
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
                                  (r.Status == (int)Record.StatusType.Pending || r.Status == (int)Record.StatusType.Running)
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
            return 60000;
        }
    }
}