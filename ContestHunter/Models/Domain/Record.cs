using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContestHunter.Database;

namespace ContestHunter.Models.Domain
{
    public class Record
    {
        public string User;
        public string Contest;
        public string Problem;
        public string Code;
        public string Detail;
        
        public enum LanguageType
        {
            C,
            CPP,
            Pascal,
        };
        public LanguageType Language;
        public DateTime SubmitTime;

        public TimeSpan ExecutedTime;

        long? Memory;
        int CodeLength;

        public enum StatusType
        {
            Accept,
            Wrong_Answer,
            Time_Limit_Execeeded,
            Runtime_Error,
            Memory_Limit_Execeeded,
            CMP_Error,
            Output_Limit_Execeeded,
            System_Error=-1,
            Pending=-2
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
        public static List<Record> Select(int skip, int top, string user, string problem, string contest, LanguageType? language, StatusType? status,OrderByType? order)
        {
            using (var db = new CHDB())
            {
                var records = (from r in db.RECORDs
                               where (null == user ? true : r.USER1.Name == user) &&
                               (null == problem ? true : r.PROBLEM1.Name == problem) &&
                               (null == contest ? true : r.PROBLEM1.CONTEST1.Name == contest) &&
                               (null == language ? true : r.Language == (int)language) &&
                               (null == status ? true : r.Status == (int)status)
                               select r);
                switch (order)
                {
                    case OrderByType.CodeLength:
                        records.OrderBy(r => r.CodeLength);
                        break;
                    case OrderByType.ExecutedTime:
                        records.OrderBy(r => r.ExecutedTime);
                        break;
                    case OrderByType.MemoryUsed:
                        records.OrderBy(r => r.MemoryUsed);
                        break;
                    case OrderByType.SubmitTime:
                        records.OrderBy(r => r.SubmitTime);
                        break;
                    default:
                        records.OrderBy(r => r.ID);
                }
                return (from r in records.Skip(skip).Take(top).ToList()
                        select new Record
                        {
                            CodeLength = r.CodeLength,
                            ExecutedTime = TimeSpan.FromMilliseconds((double)r.ExecutedTime),
                            Contest = r.PROBLEM1.CONTEST1.Name,
                            Language = (LanguageType)r.Language,
                            Memory = r.MemoryUsed,
                            Problem = r.PROBLEM1.Name,
                            Status = (StatusType)r.Status,
                            SubmitTime = r.SubmitTime,
                            User = r.USER1.Name
                        }).ToList();
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
                if (DateTime.Now <= result.PROBLEM1.CONTEST1.EndTime)
                    throw new ContestNotEndedException();
                return new Record()
                {
                    Code = result.Code,
                    CodeLength = result.CodeLength,
                    Contest = result.PROBLEM1.CONTEST1.Name,
                    Detail = result.Detail,
                    ExecutedTime = TimeSpan.FromMilliseconds((double)result.ExecutedTime),
                    Language = (LanguageType)result.Language,
                    Memory = result.MemoryUsed,
                    Problem = result.PROBLEM1.Name,
                    Status = (StatusType)result.Status,
                    SubmitTime = result.SubmitTime,
                    User = result.USER1.Name
                };
            }
        }
    }
}