using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContestHunter.Database;

namespace ContestHunter.Models.Domain
{
    public class Problem
    {
        public string Name;
        public string Content;
        public string Comparer;
        public string Contest;
        public string DataChecker;
        public int? OriginRating;
        public string Owner;
        public Record.LanguageType? ComparerLanguage;
        public Record.LanguageType? DataCheckerLanguage;

        internal bool privillege;

        internal Guid ID;
        internal Contest contest;

        public class ProblemStatus
        {
            public int SubmitUsers;
            public int PassedUsers;
        }

        /*
        public override bool Equals(object obj)
        {
            if (obj is Problem)
                return ID == ((Problem)obj).ID;
            return base.Equals(obj);
        }*/

        /// <summary>
        /// 返回题目所有测试数据编号
        /// </summary>
        /// <returns></returns>
        public List<Guid> TestCases()
        {
            using (var db = new CHDB())
            {
                return (from t in db.TESTDATAs
                        where t.PROBLEM1.ID == ID
                        select t.ID).ToList();
            }
        }

        /// <summary>
        /// 增加一组测试数据
        /// </summary>
        /// <param name="testcase"></param>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="PermissionDeniedException"></exception>
        public Guid AddTestCase(TestCase testCase)
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            if (!contest.Owner.Contains(User.CurrentUser.name) &&
                !User.CurrentUser.IsAdmin)
                throw new PermissionDeniedException();
            using (var db = new CHDB())
            {
                var result = db.TESTDATAs.Add(new TESTDATA()
                {
                    ID = Guid.NewGuid(),
                    Input = testCase._Input,
                    Data = testCase._Data,
                    TimeLimit = testCase.TimeLimit,
                    MemoryLimit = testCase.MemoryLimit,
                    PROBLEM1 = (from p in db.PROBLEMs
                                where p.ID == ID
                                select p).Single(),
                    Available = testCase.Available
                });
                db.SaveChanges();
                return result.ID;
            }
        }

        /// <summary>
        /// 删除指定测试数据
        /// </summary>
        /// <param name="testCase"></param>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="PermissionDeniedException"></exception>
        public void RemoveTestCase(Guid testCase)
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            if (!contest.Owner.Contains(User.CurrentUser.name) &&
                !User.CurrentUser.IsAdmin)
                throw new PermissionDeniedException();
            using (var db = new CHDB())
            {
                db.TESTDATAs.Remove((from t in db.TESTDATAs
                                     where t.ID == testCase
                                     select t).Single()
                                     );
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获得指定测试数据内容
        /// </summary>
        /// <param name="testCase"></param>
        /// <returns></returns>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="ContestNotEndedException"></exception>
        /// <exception cref="TestCaseNotFoundException"></exception>
        public TestCase TestCaseByID(Guid testCase)
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            bool flag = false;
            if (contest.Owner.Contains(User.CurrentUser.name) || User.CurrentUser.IsAdmin)
                flag = true;
            using (var db = new CHDB())
            {
                var currprob = (from p in db.PROBLEMs
                                where p.ID == ID
                                select p).Single();
                var currcon = currprob.CONTEST1;
                if (!flag)
                {
                    if (DateTime.Now > currcon.EndTime)
                        flag = true;
                }
                var result = (from t in currprob.TESTDATAs
                              where t.ID == testCase
                              select new TestCase
                              {
                                  ID = t.ID,
                                  canGetData = flag,
                                  TimeLimit = t.TimeLimit,
                                  MemoryLimit = t.MemoryLimit,
                                  Available = t.Available
                              }).SingleOrDefault();
                if (null == result)
                    throw new TestCaseNotFoundException();
                return result;
            }
        }

        DateTime GetVirtualSubmitTime(PROBLEM currpro)
        {
            try
            {
                if (contest.GetAttendType() == Domain.Contest.AttendType.Virtual)
                    return currpro.CONTEST1.StartTime + (DateTime.Now - contest.RelativeStartTime);
            }
            catch
            {
            }
            return DateTime.Now;
        }

        /// <summary>
        /// 提交题目 record只需要填充 Code 和 Language
        /// </summary>
        /// <param name="record"></param>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="ContestNotStartedException"></exception>
        /// <exception cref="NotAttendedContestException"></exception>
        /// <exception cref="ProblemLockedException"></exception>
        public Guid Submit(Record record)
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            using (var db = new CHDB())
            {
                (from u in db.USERs
                 where u.ID == User.CurrentUser.ID
                 select u).Single().PreferLanguage = (int)record.Language;
                var currpro = (from p in db.PROBLEMs
                               where p.ID == ID
                               select p).Single();
                if (!contest.Owner.Contains(User.CurrentUser.name) && !User.CurrentUser.IsAdmin)
                {
                    if (DateTime.Now < contest.RelativeStartTime)
                        throw new ContestNotStartedException();
                    if (!contest.IsAttended())
                        throw new NotAttendedContestException();
                }
                if (contest.Type == Domain.Contest.ContestType.CF && IsLock() && contest.RelativeNow < contest.RelativeEndTime)
                    throw new ProblemLockedException();
                Guid ret;
                db.RECORDs.Add(new RECORD()
                {
                    Code = record.Code,
                    CodeLength = record.Code.Length,
                    Detail = null,
                    ExecutedTime = null,
                    ID = ret = Guid.NewGuid(),
                    Language = (int)record.Language,
                    MemoryUsed = null,
                    PROBLEM1 = (from p in db.PROBLEMs
                                where p.ID == ID
                                select p).Single(),
                    USER1 = (from u in db.USERs
                             where u.Name == User.CurrentUser.name
                             select u).Single(),
                    Status = (int)Record.StatusType.Pending,
                    SubmitTime = DateTime.Now,
                    VirtualSubmitTime = GetVirtualSubmitTime(currpro)
                });
                db.SaveChanges();
                return ret;
            }

        }

        /// <summary>
        /// 锁定题目
        /// </summary>
        /// <exception cref="AttendedNotNormalException"></exception>
        /// <exception cref="ContestTypeMismatchException"></exception>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="NotAttendedContestException"></exception>
        /// <exception cref="ProblemNotPassedException"></exception>
        public void Lock()
        {
            if (contest.GetAttendType() != Domain.Contest.AttendType.Normal)
                throw new AttendedNotNormalException();
            if (contest.Type != Domain.Contest.ContestType.CF)
                throw new ContestTypeMismatchException();
            if (contest.RelativeNow > contest.RelativeEndTime)
                throw new ContestEndedException();
            using (var db = new CHDB())
            {
                if (!(from r in db.RECORDs
                      where r.USER1.ID == User.CurrentUser.ID
                      && r.Status == (int)Record.StatusType.Accept
                      && r.PROBLEM1.ID == ID
                      select r).Any())
                    throw new ProblemNotPassedException();
                (from u in db.USERs
                 where u.ID==User.CurrentUser.ID
                 select u).Single().LOCKs.Add((from p in db.PROBLEMs
                                               where p.ID==ID
                                               select p).Single());
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 判断本题目是否被锁定
        /// </summary>
        /// <returns></returns>
        public bool IsLock()
        {
            using (var db = new CHDB())
            {
                return (from l in
                            (from u in db.USERs
                             where u.ID==User.CurrentUser.ID
                             select u.LOCKs).Single()
                        where l.ID==ID
                        select l).Any();
            }
        }

        /// <summary>
        /// 修改Problem内容 把 Name OriginRating Content Comparer DataChecker Owner DataCheckerLanguage ComparerLanguage 更新
        /// </summary>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="PermissionDeniedException"></exception>
        /// <exception cref="UserNotFoundException"></exception>
        /// <exception cref="ProblemNameExistedException"></exception>
        public void Change()
        {
            if(null==User.CurrentUser)
                throw new UserNotLoginException();
            if (!contest.Owner.Contains(User.CurrentUser.name) &&
                !User.CurrentUser.IsAdmin)
                throw new PermissionDeniedException();
            using (var db = new CHDB())
            {
                Name = Helper.GetLegalName(Name);
                var pro = (from p in db.PROBLEMs
                           where p.ID==ID
                           select p).Single();
                var owner = (from u in db.USERs
                             where u.Name == Owner
                             select u).SingleOrDefault();
                if (null == owner)
                    throw new UserNotFoundException();
                if (pro.Name != Name && (from p in db.PROBLEMs
                                         where p.Name == Name && p.CONTEST1.ID == contest.ID
                                         select p).Any())
                    throw new ProblemNameExistedException();
                pro.Name = Name;
                pro.OriginRating = OriginRating;
                pro.Content = Content;
                pro.Comparer = Comparer;
                pro.DataChecker = DataChecker;
                pro.OWNER = owner;
                pro.DataCheckerLanguage = (int)DataCheckerLanguage;
                pro.ComparerLanguage = (int)ComparerLanguage;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 返回题目 Status OI赛制在比赛未结束的时候有异常
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ContestNotEndedException"></exception>
        public ProblemStatus Status()
        {
            if (contest.Type == Domain.Contest.ContestType.OI && contest.RelativeNow <= contest.RelativeEndTime && !User.CurrentUser.IsAdmin && !contest.Owner.Contains(User.CurrentUserName))
            {
                throw new ContestNotEndedException();
            }
            using (var db = new CHDB())
            {
                var s = (from r in db.RECORDs
                         where r.PROBLEM1.ID == ID &&
                         r.VirtualSubmitTime<=contest.RelativeNow
                         let isAC= r.Status==(int)Record.StatusType.Accept
                         select new
                         {
                             isAC
                         });
                return new ProblemStatus
                {
                    SubmitUsers = s.Count(),
                    PassedUsers = s.Sum(x => x.isAC ? 1 : 0)
                };
            }
        }

        /// <summary>
        /// 返回当前用户是否通过某题目
        /// </summary>
        /// <returns></returns>
        public bool isPassed()
        {
            using (var db = new CHDB())
            {
                return (from r in db.RECORDs
                        where r.PROBLEM1.ID == ID
                        && r.Status == (int)Record.StatusType.Accept
                        && r.USER1.ID==User.CurrentUser.ID
                        select r).Any();
            }
        }
    }
}