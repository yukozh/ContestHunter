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
            Compile_Error = -3,
            Running = -4
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
            bool privillege;
            Guid uid;
            if (null == Domain.User.CurrentUser)
            {
                uid = Guid.Empty;
                privillege = false;
            }
            else
            {
                uid = Domain.User.CurrentUser.ID;
                privillege = Domain.User.CurrentUser.IsAdmin;
            }
            using (var db = new CHDB())
            {
                return (from r in db.RecordList(top, skip, user, problem, contest, (int?)status, (int?)language, (int?)order, privillege, uid)
                        select new Record
                        {
                            CodeLength = (int)r.CodeLength,
                            Contest = r.Contest,
                            ExecutedTime = null != r.ExecutedTime ? (TimeSpan?)TimeSpan.FromMilliseconds((int)r.ExecutedTime) : null,
                            ID = (Guid)r.ID,
                            Language = (LanguageType)r.Language,
                            Memory = r.MemoryUsed,
                            Problem = r.Problem,
                            Score = r.Score,
                            Status = (StatusType?)r.Status,
                            SubmitTime = (DateTime)r.SubmitTime,
                            User = r.User
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
        /// <exception cref="ProblemNotLockedException"></exception>
        /// <exception cref="ProblemNotPassedException"></exception>
        /// <exception cref="UserNotLoginException"></exception>
        public static Record ByID(Guid id)
        {
            if (null == Domain.User.CurrentUser)
                throw new UserNotLoginException();
            using (var db = new CHDB())
            {
                var result = (from r in db.RECORDs
                              where r.ID == id
                              select r).SingleOrDefault();
                if (null == result)
                    throw new RecordNotFoundException();
                var con = Domain.Contest.ByName(result.PROBLEM1.CONTEST1.Name);
                if (!Domain.User.CurrentUser.IsAdmin && !con.Owner.Contains(Domain.User.CurrentUserName))
                {
                    bool isAttend = con.IsAttended();
                    bool isDuring = DateTime.Now <= con.RelativeEndTime;
                    switch (con.Type)
                    {
                        case Domain.Contest.ContestType.CF:
                            if (result.USER1.Name != Domain.User.CurrentUser.name && isDuring)
                            {
                                if (!(from l in
                                          (from u in db.USERs
                                           where u.Name == Domain.User.CurrentUser.name
                                           select u).Single().LOCKs
                                      where l == result.PROBLEM1
                                      select l).Any())
                                    throw new ProblemNotLockedException();
                                if (!(from r in db.RECORDs
                                      where r.USER1.ID == Domain.User.CurrentUser.ID
                                      && r.PROBLEM1.ID == result.PROBLEM1.ID
                                      && (r.Status == (int)Record.StatusType.Accept || r.Status==(int)Record.StatusType.Hacked)
                                      select r).Any())
                                    throw new ProblemNotPassedException();
                            }
                            return new Record()
                            {
                                ID = result.ID,
                                Code = isAttend ? result.Code : null,
                                CodeLength = result.CodeLength,
                                Contest = result.PROBLEM1.CONTEST1.Name,
                                Detail = result.Detail,
                                ExecutedTime = result.ExecutedTime == null ? null : (TimeSpan?)TimeSpan.FromMilliseconds((double)result.ExecutedTime),
                                Language = (LanguageType)result.Language,
                                Memory = result.MemoryUsed,
                                Problem = result.PROBLEM1.Name,
                                Status = (StatusType)result.Status,
                                SubmitTime = result.SubmitTime,
                                User = result.USER1.Name
                            };
                        case Domain.Contest.ContestType.ACM:
                            if (result.USER1.Name != Domain.User.CurrentUser.name && isDuring)
                                throw new ContestNotEndedException();
                            return new Record()
                            {
                                ID = result.ID,
                                Code = isAttend ? result.Code : null,
                                CodeLength = result.CodeLength,
                                Contest = result.PROBLEM1.CONTEST1.Name,
                                Detail = result.Detail,
                                ExecutedTime = result.ExecutedTime == null ? null : (TimeSpan?)TimeSpan.FromMilliseconds((double)result.ExecutedTime),
                                Language = (LanguageType)result.Language,
                                Memory = result.MemoryUsed,
                                Problem = result.PROBLEM1.Name,
                                Status = (StatusType)result.Status,
                                SubmitTime = result.SubmitTime,
                                User = result.USER1.Name
                            };
                        case Domain.Contest.ContestType.OI:
                            if (result.USER1.Name != Domain.User.CurrentUser.name && isDuring)
                                throw new ContestNotEndedException();
                            if (isDuring)
                            {
                                return new Record()
                                {
                                    ID = result.ID,
                                    Code = isAttend ? result.Code : null,
                                    CodeLength = result.CodeLength,
                                    Contest = result.PROBLEM1.CONTEST1.Name,
                                    Language = (LanguageType)result.Language,
                                    Problem = result.PROBLEM1.Name,
                                    SubmitTime = result.SubmitTime,
                                    User = result.USER1.Name,
                                    Detail = (result.Status == (int)StatusType.Compile_Error ? result.Detail : null),
                                    Status = (StatusType)result.Status == StatusType.Compile_Error ? (StatusType?)result.Status : null
                                };
                            }
                            else
                            {
                                return new Record()
                                {
                                    ID = result.ID,
                                    Code = isAttend ? result.Code : null,
                                    CodeLength = result.CodeLength,
                                    Contest = result.PROBLEM1.CONTEST1.Name,
                                    Language = (LanguageType)result.Language,
                                    Problem = result.PROBLEM1.Name,
                                    SubmitTime = result.SubmitTime,
                                    User = result.USER1.Name,
                                    Status = (StatusType)result.Status,
                                    Score = result.Score,
                                    ExecutedTime = result.ExecutedTime == null ? null : (TimeSpan?)TimeSpan.FromMilliseconds((double)result.ExecutedTime),
                                    Memory = result.MemoryUsed,
                                    Detail = result.Detail
                                };
                            }
                        default:
                            throw new NotImplementedException();
                    }
                }
                else
                {
                    switch (con.Type)
                    {
                        case Domain.Contest.ContestType.OI:
                            return new Record()
                            {
                                ID = result.ID,
                                Code = result.Code,
                                CodeLength = result.CodeLength,
                                Contest = result.PROBLEM1.CONTEST1.Name,
                                Language = (LanguageType)result.Language,
                                Problem = result.PROBLEM1.Name,
                                SubmitTime = result.SubmitTime,
                                User = result.USER1.Name,
                                Detail = result.Detail,
                                Score = result.Score,
                                ExecutedTime = TimeSpan.FromMilliseconds(result.ExecutedTime ?? 0),
                                Memory = result.MemoryUsed,
                                Status = (StatusType)result.Status
                            };
                        case Domain.Contest.ContestType.CF:
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
                                User = result.USER1.Name
                            };
                        case Domain.Contest.ContestType.ACM:
                            return new Record()
                            {
                                ID = result.ID,
                                Code = result.Code,
                                CodeLength = result.CodeLength,
                                Contest = result.PROBLEM1.CONTEST1.Name,
                                ExecutedTime = result.ExecutedTime == null ? null : (TimeSpan?)TimeSpan.FromMilliseconds((double)result.ExecutedTime),
                                Language = (LanguageType)result.Language,
                                Memory = result.MemoryUsed,
                                Problem = result.PROBLEM1.Name,
                                Status = (StatusType)result.Status,
                                SubmitTime = result.SubmitTime,
                                User = result.USER1.Name,
                                Detail=result.Detail
                            };
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }

        /// <summary>
        /// 返回记录总条数
        /// </summary>
        /// <returns></returns>
        public static int Count(string user, string problem, string contest, LanguageType? language, StatusType? status)
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
                return records.Count();

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
        /// <exception cref="ProblemNotPassedException"></exception>
        /// <exception cref="HuntSelfException"></exception>
        public Guid Hunt(string Data, LanguageType Type)
        {
            using (var db = new CHDB())
            {
                var curRecord = (from r in db.RECORDs
                                 where r.ID == ID
                                 select r).Single();
                if (curRecord.USER1.ID == Domain.User.CurrentUser.ID)
                    throw new HuntSelfException();
                if (curRecord.Status != (int)Record.StatusType.Accept || (from r in db.RECORDs
                                                                          where r.USER1.ID == curRecord.USER1.ID
                                                                          && r.PROBLEM1.ID == curRecord.PROBLEM1.ID
                                                                          && (r.Status == (int)Record.StatusType.Accept || r.Status == (int)Record.StatusType.Hacked)
                                                                          orderby r.VirtualSubmitTime descending
                                                                          select r).First().ID != curRecord.ID)
                    throw new RecordStatusMismatchException();
                var curContest = Domain.Contest.ByName(curRecord.PROBLEM1.CONTEST1.Name);
                if (curContest.Type != Domain.Contest.ContestType.CF)
                    throw new ContestTypeMismatchException();
                if (DateTime.Now > curContest.AbsoluteEndTime)
                    throw new ContestEndedException();
                var curProbelm = curContest.ProblemByName(curRecord.PROBLEM1.Name);
                if (!curProbelm.IsLock())
                    throw new ProblemNotLockedException();
                /*
                if (!(from r in db.RECORDs
                      where r.USER1.ID == Domain.User.CurrentUser.ID
                      && r.PROBLEM1.ID == curProbelm.ID
                      && r.Status == (int)Record.StatusType.Accept
                      select r).Any())
                    throw new ProblemNotPassedException();
                 * */
                Guid newid = Guid.NewGuid();
                db.HUNTs.Add(new HUNT()
                {
                    ID = newid,
                    HuntData = Data,
                    DataType = (int)Type,
                    RECORD1 = curRecord,
                    User = Domain.User.CurrentUser.ID,
                    Status = (int)Domain.Hunt.StatusType.Pending,
                    Time = DateTime.Now
                });
                db.SaveChanges();
                return newid;
            }
        }

        /// <summary>
        /// 返回本记录是否可以被当前用户Hunt
        /// </summary>
        /// <returns></returns>
        public bool CanHunt()
        {
            using (var db = new CHDB())
            {

                var curRecord = (from r in db.RECORDs
                                 where r.ID == ID
                                 select r).Single();
                if (curRecord.USER1.ID == Domain.User.CurrentUser.ID)
                    return false;
                if (curRecord.Status != (int)Record.StatusType.Accept || (from r in db.RECORDs
                                                                          where r.USER1.ID == curRecord.USER1.ID
                                                                          && r.PROBLEM1.ID == curRecord.PROBLEM1.ID
                                                                          && (r.Status == (int)Record.StatusType.Accept || r.Status == (int)Record.StatusType.Hacked)
                                                                          orderby r.VirtualSubmitTime descending
                                                                          select r).First().ID != curRecord.ID)
                    return false;
                var curContest = Domain.Contest.ByName(curRecord.PROBLEM1.CONTEST1.Name);
                if (curContest.Type != Domain.Contest.ContestType.CF)
                    return false;
                if (DateTime.Now > curContest.AbsoluteEndTime)
                    return false;
                var curProbelm = curContest.ProblemByName(curRecord.PROBLEM1.Name);
                if (!curProbelm.IsLock())
                    return false;
                if (!(from r in db.RECORDs
                      where r.USER1.ID == Domain.User.CurrentUser.ID
                      && r.PROBLEM1.ID == curProbelm.ID
                      && (r.Status == (int)Record.StatusType.Accept || r.Status == (int)Record.StatusType.Hacked)
                      select r).Any())
                    return false;
            }
            return true;
        }
        /// <summary>
        /// 重测指定记录
        /// </summary>
        /// <exception cref="PermissionDeniedException"></exception>
        /// <exception cref="UserNotLoginException"></exception>
        public void ReJudge()
        {
            if (null == Domain.User.CurrentUser)
                throw new UserNotLoginException();
            using (var db = new CHDB())
            {
                var curRecord = (from r in db.RECORDs
                                 where r.ID == ID
                                 select r).Single();
                var curContest = Domain.Contest.ByName(curRecord.PROBLEM1.CONTEST1.Name);
                if (!Domain.User.CurrentUser.IsAdmin && !curContest.Owners.Contains(Domain.User.CurrentUser.Name))
                    throw new PermissionDeniedException();
                (from r in db.RECORDs
                 where r.ID == ID
                 select r).Single().Status = (int)StatusType.Pending;
                db.SaveChanges();
            }
        }

        public bool ShouldShowImage()
        {
            var con = Domain.Contest.ByName(Contest);
            if (con.RelativeNow <= con.RelativeEndTime && !Domain.User.CurrentUser.IsAdmin
                && !con.Owner.Contains(Domain.User.CurrentUserName) && (null == Domain.User.CurrentUser || User != Domain.User.CurrentUserName))
                return true;
            return false;

        }
    }
}