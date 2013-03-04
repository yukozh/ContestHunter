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
            DataCheckerError,
            HackedByOther,
            OtherError = -1
        }

        public string User;
        public string Contest;
        public string Problem;
        public string Data;
        public string UserBeHunted;
        public Guid ID;

        public Guid Record;
        public StatusType Status;
        public string Detail;
        public DateTime Time;
        public Record.LanguageType DataType;

        public static List<Hunt> List(int top, int skip, string user, string contest, string problem, StatusType? status)
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
                if (null != status)
                    ht = ht.Where(x => x.Status == (int)status);
                ht.OrderByDescending(x => x.Time).Skip(skip).Take(top);
                List<Hunt> Ret = new List<Hunt>();
                foreach (var h in ht)
                {
                    Ret.Add(new Hunt()
                    {
                        ID = h.ID,
                        Contest = h.RECORD1.PROBLEM1.CONTEST1.Name,
                        Problem = h.RECORD1.PROBLEM1.Name,
                        Record = h.RECORD1.ID,
                        Status = (StatusType)h.Status,
                        User = h.USER1.Name,
                        Time = h.Time,
                        Detail = h.Detail,
                        DataType = (Record.LanguageType)h.DataType,
                        UserBeHunted = h.RECORD1.USER1.Name
                    });
                }
                return Ret.OrderByDescending(x=>x.Time).ToList();
            }
        }

        public static int Count(string user, string contest, string problem, StatusType? status)
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
                if (null != status)
                    ht = ht.Where(x => x.Status == (int)status);
                return ht.Count();
            }
        }

        /// <summary>
        /// 返回指定ID的Hunt结果
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        /// <exception cref="UserNotLoginException"></exception>
        public static Hunt ByID(Guid ID)
        {
            if (null == Domain.User.CurrentUser)
                throw new UserNotLoginException();
            using (var db = new CHDB())
            {
                return (from h in db.HUNTs
                        where h.ID == ID
                        let flag=Domain.User.CurrentUser.ID == h.USER1.ID || Domain.User.CurrentUser.IsAdmin || h.RECORD1.PROBLEM1.CONTEST1.OWNERs.Where(x=>x.Name==Domain.User.CurrentUserName).Any()
                        select new Hunt()
                        {
                            Contest = h.RECORD1.PROBLEM1.CONTEST1.Name,
                            Problem = h.RECORD1.PROBLEM1.Name,
                            Record = h.RECORD1.ID,
                            Status = (StatusType)h.Status,
                            User = h.USER1.Name,
                            Time = h.Time,
                            Data = ( flag ? h.HuntData : null),
                            ID = h.ID,
                            Detail = h.Detail,
                            DataType = (Record.LanguageType)h.DataType,
                            UserBeHunted = h.RECORD1.USER1.Name
                        }).Single();
            }
        }
    }
}