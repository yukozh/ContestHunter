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
            CompileError,
            OtherError
        }

        public string User;
        public string Contest;
        public string Problem;
        public string Data;
        public Guid ID;

        public Guid Record;
        public StatusType Status;
        public string Detail;
        public DateTime Time;
        public Record.LanguageType DataType;

        public static List<Hunt> Get(int top,int skip,string user, string contest,string problem)
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
                ht.OrderByDescending(x => x.Time).Skip(skip).Take(top);
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
                        Time = h.Time,
                        Detail = h.Detail,
                        DataType = (Record.LanguageType)h.DataType
                    });
                }
                return Ret;
            }
        }

        public static Hunt ByID(Guid ID)
        {
            using (var db = new CHDB())
            {
                return (from h in db.HUNTs
                        where h.ID == ID
                        select new Hunt()
                        {
                            Contest = h.RECORD1.PROBLEM1.CONTEST1.Name,
                            Problem = h.RECORD1.PROBLEM1.Name,
                            Record = h.RECORD1.ID,
                            Status = (StatusType)h.Status,
                            User = h.USER1.Name,
                            Time = h.Time,
                            Data = (Domain.User.CurrentUser.ID == h.USER1.ID ? h.HuntData : null),
                            ID = h.ID,
                            Detail = h.Detail,
                            DataType = (Record.LanguageType)h.DataType
                        }).Single();
            }
        }
    }
}