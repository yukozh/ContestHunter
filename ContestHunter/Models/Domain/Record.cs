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
            Java,
            Data
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
            Hacked,
            System_Error = -1,
            Pending = -2,
            Compile_Error = -3
        }

        public StatusType? Status;

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
                    if (DateTime.Now <= Domain.Contest.ByName(r.PROBLEM1.CONTEST1.Name).RelativeEndTime)
                    {
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
                    }
                    else
                    {
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
                            case (int)Domain.Contest.ContestType.OI:
                                nrec.Score = r.Score;
                                nrec.ExecutedTime = r.ExecutedTime == null ? null : (TimeSpan?)TimeSpan.FromMilliseconds((double)r.ExecutedTime);
                                nrec.Memory = r.MemoryUsed;
                                break;
                        }
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
        /// <exception cref="ProblemNotLockedException"></exception>
        public static Record ByID(Guid id)
        {
            using (var db = new CHDB())
            {
                var result = (from r in db.RECORDs
                              where r.ID == id
                              select r).SingleOrDefault();
                if (null == result)
                    throw new RecordNotFoundException();
                if (result.USER1.Name != Domain.User.CurrentUser.name)
                {
                    var con = Domain.Contest.ByName(result.PROBLEM1.CONTEST1.Name);
                    if (DateTime.Now <= Domain.Contest.ByName(result.PROBLEM1.CONTEST1.Name).RelativeEndTime)
                    {
                        if (con.Type != Domain.Contest.ContestType.CF)
                        {
                            throw new ContestNotEndedException();
                        }
                        else
                        {
                            if (!(from l in
                                      (from u in db.USERs
                                       where u.Name == Domain.User.CurrentUser.name
                                       select u).Single().LOCKs
                                  where l == result.PROBLEM1
                                  select l).Any())
                                throw new ProblemNotLockedException();
                        }
                    }
                }
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

        /// <summary>
        /// Hunt本记录
        /// </summary>
        /// <param name="Data"></param>
        /// <exception cref="RecordStatusMismatchException"></exception>
        /// <exception cref="ContestTypeMismatchException"></exception>
        /// <exception cref="ContestEndedException"></exception>
        /// <exception cref="ProblemNotLockedException"></exception>
        public void Hunt(string Data,LanguageType Type)
        {
            using (var db = new CHDB())
            {
                var curRecord = (from r in db.RECORDs
                           where r.ID == ID
                           select r).Single();
                if (curRecord.Status != (int)Record.StatusType.Accept)
                    throw new RecordStatusMismatchException();
                var curContest = Domain.Contest.ByName(curRecord.PROBLEM1.CONTEST1.Name);
                if (curContest.Type != Domain.Contest.ContestType.CF)
                    throw new ContestTypeMismatchException();
                if (DateTime.Now > curContest.AbsoluteEndTime)
                    throw new ContestEndedException();
                var curProbelm = curContest.ProblemByName(curRecord.PROBLEM1.Name);
                if (!curProbelm.IsLock())
                    throw new ProblemNotLockedException();
                db.HUNTs.Add(new HUNT()
                {
                    HuntData = Data,
                    DataType = (int)Type,
                    RECORD1 = curRecord,
                    User = Domain.User.CurrentUser.ID,
                    Status = (int)Domain.Hunt.StatusType.Pending,
                    Time = DateTime.Now
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 重测指定记录
        /// </summary>
        /// <exception cref="PermissionDeniedException"></exception>
        public void ReJudge()
        {
            if (!Domain.User.CurrentUser.IsAdmin)
                throw new PermissionDeniedException();
            using (var db = new CHDB())
            {
                (from r in db.RECORDs
                 where r.ID == ID
                 select r).Single().Status = (int)StatusType.Pending;
            }
        }
    }
}