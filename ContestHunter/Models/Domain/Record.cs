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
                var tmp = db.RecordList(top, skip, user, problem, contest, (int?)status, (int?)language, (int?)order);
                List<Record> Ret = new List<Record>();
                foreach (RecordList_Result r in tmp)
                {
                    var nrec = new Record()
                    {
                        ID = r.ID,
                        CodeLength = r.CodeLength,
                        Contest = r.Contest,
                        Language = (LanguageType)r.Language,
                        Problem = r.Problem,
                        SubmitTime = r.SubmitTime,
                        User = r.User
                    };
                    var con=Domain.Contest.ByName(r.Contest);
                    if (DateTime.Now <= con.RelativeEndTime && ( null==Domain.User.CurrentUser || (!Domain.User.CurrentUser.IsAdmin && !con.Owner.Contains(Domain.User.CurrentUserName))))
                    {
                        switch (con.Type)
                        {
                            case Domain.Contest.ContestType.CF:
                                nrec.Status = (StatusType)r.Status;
                                nrec.ExecutedTime = r.ExecutedTime == null ? null : (TimeSpan?)TimeSpan.FromMilliseconds((double)r.ExecutedTime);
                                nrec.Memory = r.MemoryUsed;
                                break;
                            case Domain.Contest.ContestType.ACM:
                                nrec.Status = (StatusType)r.Status;
                                nrec.ExecutedTime = r.ExecutedTime == null ? null : (TimeSpan?)TimeSpan.FromMilliseconds((double)r.ExecutedTime);
                                nrec.Memory = r.MemoryUsed;
                                break;
                            case Domain.Contest.ContestType.OI:
                                if (r.Status==(int)StatusType.Compile_Error)
                                {
                                    nrec.Status = StatusType.Compile_Error;
                                }
                                break;
                        }
                    }
                    else
                    {
                        switch (con.Type)
                        {
                            case Domain.Contest.ContestType.CF:
                                nrec.Status = (StatusType)r.Status;
                                nrec.ExecutedTime = r.ExecutedTime == null ? null : (TimeSpan?)TimeSpan.FromMilliseconds((double)r.ExecutedTime);
                                nrec.Memory = r.MemoryUsed;
                                break;
                            case Domain.Contest.ContestType.ACM:
                                nrec.Status = (StatusType)r.Status;
                                nrec.ExecutedTime = r.ExecutedTime == null ? null : (TimeSpan?)TimeSpan.FromMilliseconds((double)r.ExecutedTime);
                                nrec.Memory = r.MemoryUsed;
                                break;
                            case Domain.Contest.ContestType.OI:
                                nrec.Score = r.Score;
                                nrec.Status = (StatusType)r.Status;
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
                if (!Domain.User.CurrentUser.IsAdmin && !con.Owner.Contains(Domain.User.CurrentUserName) && DateTime.Now <= con.RelativeEndTime)
                {
                    bool isAttend = con.IsAttended();
                    switch (con.Type)
                    {
                        case Domain.Contest.ContestType.CF:
                            if (result.USER1.Name != Domain.User.CurrentUser.name)
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
                                      && r.Status == (int)Record.StatusType.Accept
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
                            if (result.USER1.Name != Domain.User.CurrentUser.name)
                                throw new ContestNotEndedException();
                            return new Record()
                            {
                                ID = result.ID,
                                Code = isAttend ? result.Code : null,
                                CodeLength = result.CodeLength,
                                Contest = result.PROBLEM1.CONTEST1.Name,
                                ExecutedTime = result.ExecutedTime == null ? null : (TimeSpan?)TimeSpan.FromMilliseconds((double)result.ExecutedTime),
                                Language = (LanguageType)result.Language,
                                Memory = result.MemoryUsed,
                                Problem = result.PROBLEM1.Name,
                                Status = (StatusType)result.Status,
                                SubmitTime = result.SubmitTime,
                                User = result.USER1.Name
                            };
                        case Domain.Contest.ContestType.OI:
                            if (result.USER1.Name != Domain.User.CurrentUser.name)
                                throw new ContestNotEndedException();
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
                                Detail = result.Status == (int)StatusType.Compile_Error ? result.Detail : null
                            };
                        default:
                            throw new NotImplementedException();
                    }
                }
                else
                {
                    bool isAttend = con.IsAttended();
                    switch (con.Type)
                    {
                        case Domain.Contest.ContestType.OI:
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
                            return new Record()
                            {
                                ID = result.ID,
                                Code = isAttend ? result.Code : null,
                                CodeLength = result.CodeLength,
                                Contest = result.PROBLEM1.CONTEST1.Name,
                                ExecutedTime = result.ExecutedTime == null ? null : (TimeSpan?)TimeSpan.FromMilliseconds((double)result.ExecutedTime),
                                Language = (LanguageType)result.Language,
                                Memory = result.MemoryUsed,
                                Problem = result.PROBLEM1.Name,
                                Status = (StatusType)result.Status,
                                SubmitTime = result.SubmitTime,
                                User = result.USER1.Name
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
        public Guid Hunt(string Data,LanguageType Type)
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
                                                                          && (r.Status == (int)Record.StatusType.Accept || r.Status==(int)Record.StatusType.Hacked)
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
                if (!(from r in db.RECORDs
                      where r.USER1.ID == Domain.User.CurrentUser.ID
                      && r.PROBLEM1.ID == curProbelm.ID
                      && r.Status == (int)Record.StatusType.Accept
                      select r).Any())
                    throw new ProblemNotPassedException();
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
                                                                          && (r.Status == (int)Record.StatusType.Accept || r.Status==(int)Record.StatusType.Hacked)
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
                      && r.Status == (int)Record.StatusType.Accept
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
            if (!Domain.User.CurrentUser.IsAdmin)
                throw new PermissionDeniedException();
            using (var db = new CHDB())
            {
                (from r in db.RECORDs
                 where r.ID == ID
                 select r).Single().Status = (int)StatusType.Pending;
                db.SaveChanges();
            }
        }

        public bool ShouldShowImage()
        {
            var con =Domain.Contest.ByName(Contest);
            if (con.RelativeNow <= con.RelativeEndTime && !Domain.User.CurrentUser.IsAdmin
                && !con.Owner.Contains(Domain.User.CurrentUserName))
                return true;
            return false;
                
        }
    }
}