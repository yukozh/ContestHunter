using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContestHunter.Database;

namespace ContestHunter.Models.Domain
{
    public class Record
    {
        public Guid ID;
        public string User;
        public string Contest;
        public string Problem;
        public string Code;
        public string Detail;
        public int? Score;

        public enum LanguageType
        {
            C,
            CPP,
            Pascal,
            Java
        };
        public LanguageType Language;
        public DateTime SubmitTime;

        public TimeSpan? ExecutedTime;

        public long? Memory;
        public int CodeLength;

        public enum StatusType
        {
            Accept,
            Wrong_Answer,
            Time_Limit_Execeeded,
            Runtime_Error,
            Memory_Limit_Execeeded,
            CMP_Error,
            Output_Limit_Execeeded,
            System_Error = -1,
            Pending = -2,
            Compile_Error = -3
        }

        public StatusType Status;

        public enum OrderByType
        {
            SubmitTime,
            CodeLength,
            MemoryUsed,
            ExecutedTime
        }

        /// <summary>
        /// 根据条件获得记录列表
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <param name="user"></param>
        /// <param name="problem"></param>
        /// <param name="contest"></param>
        /// <param name="language"></param>
        /// <param name="status"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static List<Record> Select(int skip, int top, string user, string problem, string contest, LanguageType? language, StatusType? status, OrderByType order)
        {
            using (var db = new CHDB())
            {
                IQueryable<RECORD> records = db.RECORDs;
                if (user != null)
                    records = records.Where(r => r.USER1.Name == user);
                if (problem != null)
                    records = records.Where(r => r.PROBLEM1.Name == problem);
                if (contest != null)
                    records = records.Where(r => r.PROBLEM1.CONTEST1.Name == contest);
                if (language != null)
                    records = records.Where(r => r.Language == (int)language);
                if (status != null)
                    records = records.Where(r => r.Status == (int)status);
                switch (order)
                {
                    case OrderByType.CodeLength:
                        records = records.OrderBy(r => r.CodeLength);
                        break;
                    case OrderByType.ExecutedTime:
                        records = records.OrderBy(r => r.ExecutedTime);
                        break;
                    case OrderByType.MemoryUsed:
                        records = records.OrderBy(r => r.MemoryUsed);
                        break;
                    case OrderByType.SubmitTime:
                        records = records.OrderByDescending(r => r.SubmitTime);
                        break;
                }
                var tmp = records.Skip(skip).Take(top).ToList();
                List<Record> Ret = new List<Record>();
                foreach (RECORD r in tmp)
                {
                    var nrec = new Record()
                    {
                        ID = r.ID,
                        CodeLength = r.CodeLength,
                        Contest = r.PROBLEM1.CONTEST1.Name,
                        Language = (LanguageType)r.Language,
                        Problem = r.PROBLEM1.Name,
                        SubmitTime = r.SubmitTime,
                        User = r.USER1.Name
                    };
                    switch (r.PROBLEM1.CONTEST1.Type)
                    {
                        case (int)Domain.Contest.ContestType.CF:
                            nrec.Score = r.Score;
                            nrec.Status = (StatusType)r.Status;
                            nrec.ExecutedTime = r.ExecutedTime == null ? null : (TimeSpan?)TimeSpan.FromMilliseconds((double)r.ExecutedTime);
                            nrec.Memory = r.MemoryUsed;
                            break;
                        case (int)Domain.Contest.ContestType.ACM:
                            nrec.Status = (StatusType)r.Status;
                            nrec.ExecutedTime = r.ExecutedTime == null ? null : (TimeSpan?)TimeSpan.FromMilliseconds((double)r.ExecutedTime);
                            nrec.Memory = r.MemoryUsed;
                            break;
                    }
                    Ret.Add(nrec);
                }
                return Ret;
            }
        }

        /// <summary>
        /// 获得指定ID的记录信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ContestNotEndedException"></exception>
        /// <exception cref="RecordNotFoundException"></exception>
        public static Record ByID(Guid id)
        {
            using (var db = new CHDB())
            {
                var result = (from r in db.RECORDs
                              where r.ID == id
                              select r).SingleOrDefault();
                if (null == result)
                    throw new RecordNotFoundException();
                if (result.Status != (int)Record.StatusType.Compile_Error || result.USER1.Name != Domain.User.CurrentUser.name)
                    if (DateTime.Now <= result.PROBLEM1.CONTEST1.EndTime)
                        throw new ContestNotEndedException();
                return new Record()
                {
                    ID = result.ID,
                    Code = result.Code,
                    CodeLength = result.CodeLength,
                    Contest = result.PROBLEM1.CONTEST1.Name,
                    Detail = result.Detail,
                    ExecutedTime = result.ExecutedTime == null ? null : (TimeSpan?)TimeSpan.FromMilliseconds((double)result.ExecutedTime),
                    Language = (LanguageType)result.Language,
                    Memory = result.MemoryUsed,
                    Problem = result.PROBLEM1.Name,
                    Status = (StatusType)result.Status,
                    SubmitTime = result.SubmitTime,
                    User = result.USER1.Name,
                    Score = result.Score
                };
            }
        }

        /// <summary>
        /// 返回记录总条数
        /// </summary>
        /// <returns></returns>
        public static int Count()
        {
            using (var db = new CHDB())
            {
                return (from r in db.RECORDs
                        select r).Count();
            }
        }
    }
}