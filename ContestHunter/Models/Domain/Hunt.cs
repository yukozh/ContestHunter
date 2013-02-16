using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContestHunter.Database;

namespace ContestHunter.Models.Domain
{
    public class Hunt
    {
        public enum StatusType
        {
            Pending,
            Success,
            Fail,
            BadData,
            OtherError
        }

        string User;
        string Contest;
        string Problem;
        byte[] Data;

        Guid Record;
        StatusType Status;
        string Detail;
        DateTime Time;

        public static List<Hunt> Get(string user, string contest,string problem)
        {
            using (var db = new CHDB())
            {
                var ht = from h in db.HUNTs
                         select h;
                if (null != user)
                    ht = ht.Where(x => x.USER1.Name == user);
                if (null != contest)
                    ht = ht.Where(x => x.RECORD1.PROBLEM1.CONTEST1.Name == contest);
                if (null != problem)
                    ht = ht.Where(x => x.RECORD1.PROBLEM1.Name == problem);
                List<Hunt> Ret = new List<Hunt>();
                foreach (var h in ht)
                {
                    Ret.Add(new Hunt()
                    {
                        Contest = h.RECORD1.PROBLEM1.CONTEST1.Name,
                        Problem = h.RECORD1.PROBLEM1.Name,
                        Record = h.RECORD1.ID,
                        Status = (StatusType)h.Status,
                        User = h.USER1.Name,
                        Detail = (h.USER1.Name == Domain.User.CurrentUser.name ? h.Detail : null),
                        Time = h.Time,
                        Data = h.HuntData.Length > 1024 ? h.HuntData.Take(1024).ToArray() : h.HuntData
                    });
                }
                return Ret;
            }
        }
    }
}