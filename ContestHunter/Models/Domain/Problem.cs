﻿using System;
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

        internal Guid ID;
        internal Contest contest;

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
                !User.CurrentUser.groups.Contains("Administrators"))
                throw new PermissionDeniedException();
            using (var db = new CHDB())
            {
                var result = db.TESTDATAs.Add(new TESTDATA()
                {
                    ID = Guid.NewGuid(),
                    Input = testCase.Input,
                    Data = testCase.Data,
                    TimeLimit = testCase.TimeLimit,
                    MemoryLimit=testCase.MemoryLimit,
                    PROBLEM1=(from p in db.PROBLEMs
                              where p.ID == ID
                              select p).Single()
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
                !User.CurrentUser.groups.Contains("Administrators"))
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
            if (contest.Owner.Contains(User.CurrentUser.name) || User.CurrentUser.groups.Contains("Administrators"))
                flag = true;
            using (var db = new CHDB())
            {
                var currcon = (from p in db.PROBLEMs
                               where p.ID == ID
                               select p.CONTEST1).Single();
                if (!flag)
                {
                    if (DateTime.Now < currcon.EndTime)
                        throw new ContestNotEndedException();
                }
                var result = (from t in db.TESTDATAs
                              where t.ID == testCase
                              select new TestCase
                              {
                                  Input = t.Input,
                                  Data = t.Data,
                                  TimeLimit = t.TimeLimit
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
                var currpro = (from p in db.PROBLEMs
                               where p.ID == ID
                               select p).Single();
                if (!contest.Owner.Contains(User.CurrentUser.name) && !User.CurrentUser.groups.Contains("Administrators"))
                {
                    if (DateTime.Now < contest.RelativeStartTime)
                        throw new ContestNotStartedException();
                    if (DateTime.Now <= contest.RelativeEndTime && !contest.IsAttended())
                        throw new NotAttendedContestException();
                }
                if (contest.Type == Domain.Contest.ContestType.CF && IsLock())
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
        public void Lock()
        {
            if (contest.GetAttendType() != Domain.Contest.AttendType.Normal)
                throw new AttendedNotNormalException();
            if (contest.Type != Domain.Contest.ContestType.CF)
                throw new ContestTypeMismatchException();
            using (var db = new CHDB())
            {
                (from u in db.USERs
                 where u.Name == User.CurrentUser.name
                 select u.LOCKs).Single().Add((from p in db.PROBLEMs
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
                             where u.Name == User.CurrentUser.name
                             select u.LOCKs).Single()
                        where l.ID==ID
                        select l).Any();
            }
        }

        public void Change()
        {
            if(null==User.CurrentUser)
                throw new UserNotLoginException();
            if (!contest.Owner.Contains(User.CurrentUser.name) &&
                !User.CurrentUser.groups.Contains("Administrators"))
                throw new PermissionDeniedException();
            using (var db = new CHDB())
            {
                var pro = (from p in db.PROBLEMs
                           where p.ID==ID
                           select p).Single();
                var owner = (from u in db.USERs
                             where u.Name == Owner
                             select u).SingleOrDefault();
                if (null == owner)
                    throw new UserNotFoundException();
                pro.Name = Name;
                pro.OriginRating = OriginRating;
                pro.Content = Content;
                pro.Comparer = Comparer;
                pro.DataChecker = DataChecker;
                pro.OWNER = owner;
                db.SaveChanges();
            }
        }
    }
}