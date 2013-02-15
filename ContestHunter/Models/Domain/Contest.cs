﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContestHunter.Database;

namespace ContestHunter.Models.Domain
{
    public class Contest
    {
        public string Name;
        public enum ContestType
        {
            ACM,
            CF,
            OI
        }
        public ContestType Type;
        public DateTime AbsoluteStartTime, AbsoluteEndTime;
        public DateTime RelativeStartTime
        {
            get
            {
                try
                {
                    if (GetAttendType() == AttendType.Virtual)
                        return VirtualStartTime();
                }
                catch
                {
                }
                return AbsoluteStartTime;
            }
            set
            {
                AbsoluteStartTime = value;
            }
        }
        public DateTime RelativeEndTime
        {
            get
            {
                try
                {
                    if (GetAttendType()==AttendType.Virtual)
                        return VirtualEndTime();
                }
                catch
                {
                }
                return AbsoluteEndTime;
            }
            set
            {
                AbsoluteEndTime = value;
            }
        }
        public string Description;
        public bool IsOfficial;
        public List<string> Owner;

        public enum AttendType
        {
            Normal,
            Virtual,
            Practice
        }

        /// <summary>
        /// 返回等待进行的比赛列表
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public static List<Contest> Pending(int skip, int top)
        {
            using (var db = new CHDB())
            {
                return (from c in
                            (from contest in db.CONTESTs
                             where contest.StartTime > DateTime.Now
                             select contest
                                ).OrderBy(x => x.StartTime).Skip(skip).Take(top).ToList()
                        select new Contest
                         {
                             Name = c.Name,
                             Description = c.Description,
                             Type = (ContestType)c.Type,
                             RelativeStartTime = c.StartTime,
                             RelativeEndTime = c.EndTime,
                             IsOfficial = c.IsOfficial,
                             Owner = (from u in c.OWNERs
                                      select u.Name).ToList()
                         }).ToList();
            }
        }

        /// <summary>
        /// 返回 Pending 比赛个数
        /// </summary>
        /// <returns></returns>
        public static int PendingCount()
        {
            using (var db = new CHDB())
            {
                return (from c in db.CONTESTs
                        where c.StartTime > DateTime.Now
                        select c).Count();
            }
        }

        /// <summary>
        /// 返回正在进行的比赛列表
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public static List<Contest> Testing(int skip, int top)
        {
            using (var db = new CHDB())
            {
                return (from c in
                            (from contest in db.CONTESTs
                             where contest.StartTime <= DateTime.Now && contest.EndTime >= DateTime.Now
                             select contest
                                ).OrderBy(x => x.StartTime).Skip(skip).Take(top).ToList()
                        select new Contest
                        {
                            Name = c.Name,
                            Description = c.Description,
                            Type = (ContestType)c.Type,
                            RelativeStartTime = c.StartTime,
                            RelativeEndTime = c.EndTime,
                            IsOfficial = c.IsOfficial,
                            Owner = (from u in c.OWNERs
                                     select u.Name).ToList()
                        }).ToList();
            }
        }

        /// <summary>
        /// 返回 Testing 比赛个数
        /// </summary>
        /// <returns></returns>
        public static int TestingCount()
        {
            using (var db = new CHDB())
            {
                return (from c in db.CONTESTs
                        where c.StartTime <= DateTime.Now && c.EndTime >= DateTime.Now
                        select c).Count();
            }
        }

        /// <summary>
        /// 返回已经结束的比赛列表
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public static List<Contest> Done(int skip, int top)
        {
            using (var db = new CHDB())
            {
                return (from c in
                            (from contest in db.CONTESTs
                             where contest.EndTime < DateTime.Now
                             orderby contest.StartTime descending
                             select contest
                                ).Skip(skip).Take(top).ToList()
                        select new Contest
                        {
                            Name = c.Name,
                            Description = c.Description,
                            Type = (ContestType)c.Type,
                            RelativeStartTime = c.StartTime,
                            RelativeEndTime = c.EndTime,
                            IsOfficial = c.IsOfficial,
                            Owner = (from u in c.OWNERs
                                     select u.Name).ToList()
                        }).ToList();
            }
        }

        /// <summary>
        /// 返回 Done 比赛个数
        /// </summary>
        /// <returns></returns>
        public static int DoneCount()
        {
            using (var db = new CHDB())
            {
                return (from c in db.CONTESTs
                        where c.EndTime < DateTime.Now
                        select c).Count();
            }
        }

        /// <summary>
        /// 添加比赛
        /// </summary>
        /// <param name="contest"></param>
        /// <exception cref="UserNotLoginException"></exception>
        public static void Add(Contest contest)
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            using (var db = new CHDB())
            {
                var curContest = db.CONTESTs.Add(new CONTEST()
                {
                    ID = Guid.NewGuid(),
                    Name = contest.Name,
                    StartTime = contest.RelativeStartTime,
                    EndTime = contest.RelativeEndTime,
                    Description = contest.Description,
                    Type = (int)contest.Type,
                    IsOfficial = contest.IsOfficial,
                });

                foreach (string name in contest.Owner)
                {
                    curContest.OWNERs.Add(
                        (from u in db.USERs
                         where u.Name == name
                         select u).Single()
                        );
                }

                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取指定名称比赛实例
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ContestNotFoundException"></exception>
        public static Contest ByName(string name)
        {
            using (var db = new CHDB())
            {
                var result = (from c in
                                  (from c in db.CONTESTs
                                   where c.Name == name
                                   select c).ToList()
                              select new Contest
                          {
                              Name = c.Name,
                              Description = c.Description,
                              RelativeStartTime = c.StartTime,
                              RelativeEndTime = c.EndTime,
                              IsOfficial = c.IsOfficial,
                              Type = (ContestType)c.Type,
                              Owner = (from u in c.OWNERs
                                       select u.Name).ToList()
                          }).SingleOrDefault();
                if (null == result)
                    throw new ContestNotFoundException();
                return result;
            }
        }

        /// <summary>
        /// 报名参加比赛
        /// </summary>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="AlreadyAttendedContestException"></exception>
        /// <exception cref="ContestEndedException"></exception>
        public void Attend()
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            if (IsAttended())
                throw new AlreadyAttendedContestException();
            if (DateTime.Now > RelativeEndTime)
                throw new ContestEndedException();
            using (var db = new CHDB())
            {
                var con = (from c in db.CONTESTs
                           where c.Name == Name
                           select c).Single();
                con.CONTEST_ATTEND.Add(new CONTEST_ATTEND()
                 {
                     USER1 = (from u in db.USERs
                              where u.ID == User.CurrentUser.ID
                              select u).Single(),
                     Type = (int)AttendType.Normal,
                     Time = DateTime.Now
                 });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 虚拟报名比赛
        /// </summary>
        /// <param name="startTime"></param>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="AlreadyAttendedContestException"></exception>
        /// <exception cref="ContestNotStartedException"></exception>
        public void VirtualAttend(DateTime startTime)
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            if (IsAttended())
                throw new AlreadyAttendedContestException();
            if (DateTime.Now <= RelativeStartTime)
                throw new ContestNotStartedException();
            using (var db = new CHDB())
            {
                var con = (from c in db.CONTESTs
                           where c.Name == Name
                           select c).Single();
                con.CONTEST_ATTEND.Add(new CONTEST_ATTEND()
                {
                    USER1 = (from u in db.USERs
                             where u.ID == User.CurrentUser.ID
                             select u).Single(),
                    Type = (int)AttendType.Virtual,
                    Time = startTime
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 虚拟报名比赛
        /// </summary>
        /// <param name="startTime"></param>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="AlreadyAttendedContestException"></exception>
        /// <exception cref="ContestNotEndedException"></exception>
        public void PracticeAttend()
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            if (IsAttended())
                throw new AlreadyAttendedContestException();
            if (DateTime.Now <= RelativeEndTime)
                throw new ContestNotEndedException();
            using (var db = new CHDB())
            {
                var con = (from c in db.CONTESTs
                           where c.Name == Name
                           select c).Single();
                con.CONTEST_ATTEND.Add(new CONTEST_ATTEND()
                {
                    USER1 = (from u in db.USERs
                             where u.ID == User.CurrentUser.ID
                             select u).Single(),
                    Type = (int)AttendType.Practice,
                    Time = DateTime.Now
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 取消参加比赛
        /// </summary>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="NotAttendedContestException"></exception>
        /// <exception cref="ContestStartedException"></exception>
        public void Disattended()
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            if (!IsAttended())
                throw new NotAttendedContestException();
            using (var db = new CHDB())
            {
                var con = (from c in db.CONTESTs
                           where c.Name == Name
                           select c).Single();
                var con_att = (from u in con.CONTEST_ATTEND
                               where u.USER1.Name == User.CurrentUser.name
                               select u).Single();
                if (DateTime.Now > RelativeStartTime)
                    throw new ContestStartedException();
                con.CONTEST_ATTEND.Remove(con_att);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 判断是否报名比赛
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UserNotLoginException"></exception>
        public bool IsAttended()
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            using (var db = new CHDB())
            {
                return (from u in
                            (from c in db.CONTESTs
                             where c.Name == Name
                             select c).Single().CONTEST_ATTEND
                        where u.USER1.Name == User.CurrentUser.name
                        select u).Any();
            }
        }

        /// <summary>
        /// 获得报名人数
        /// </summary>
        /// <returns></returns>
        public int AttendedUsersCount()
        {
            using (var db = new CHDB())
            {
                return (from c in db.CONTESTs
                        where c.Name == Name
                        select c.CONTEST_ATTEND).Single().Count();
            }
        }

        /// <summary>
        /// 获得比赛报名名单
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public List<string> AttendedUsers(int skip, int top)
        {
            using (var db = new CHDB())
            {
                return (from u in
                            (from c in db.CONTESTs
                             where c.Name == Name
                             select c.CONTEST_ATTEND).Single()
                        orderby u.USER1.Name ascending
                        select u.USER1.Name).Skip(skip).Take(top).ToList();

            }
        }

        /// <summary>
        /// 获取比赛题目名称列表
        /// </summary>
        /// <returns></returns>
        public List<string> Problems()
        {
            using (var db = new CHDB())
            {
                return (from p in db.PROBLEMs
                        where p.CONTEST1.Name == Name
                        select p.Name).ToList();
            }
        }

        /// <summary>
        /// 获得相应比赛的指定名称的题目
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ContestNotStartedException"></exception>
        /// <exception cref="NotAttendedContestException"></exception>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="ProblemNotFoundException"></exception>
        /// <exception cref="UserNotLoginException"></exception>
        public Problem ProblemByName(string name)
        {
            using (var db = new CHDB())
            {
                if (null == User.CurrentUser)
                {
                    if (DateTime.Now <= RelativeEndTime)
                        throw new UserNotLoginException();
                }
                else
                {
                    if (!Owner.Contains(User.CurrentUser.name) && !User.CurrentUser.groups.Contains("Administrators"))
                    {
                        var con = (from c in db.CONTESTs
                                   where c.Name == Name
                                   select c).Single();
                        if (!IsAttended())
                        {
                            if (DateTime.Now <= RelativeEndTime)
                                throw new ContestNotEndedException();
                        }
                        else
                        {
                            if (DateTime.Now < RelativeStartTime)
                                throw new ContestNotStartedException();
                        }
                    }
                }
                var result = (from p in db.PROBLEMs
                              where p.Name == name && p.CONTEST1.Name == Name
                              select p).SingleOrDefault();
                if (null == result)
                    throw new ProblemNotFoundException();

                return new Problem()
                              {
                                  Name = result.Name,
                                  Content = result.Content,
                                  Comparer = result.Comparer,
                                  ID = result.ID,
                                  Contest = result.CONTEST1.Name,
                                  contest = this
                              };
            }

        }

        /// <summary>
        /// 添加题目
        /// </summary>
        /// <param name="problem"></param>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="PermissionDeniedException"></exception>
        public void AddProblem(Problem problem)
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            if (!Owner.Contains(User.CurrentUser.name)
                && !User.CurrentUser.groups.Contains("Administrators"))
                throw new PermissionDeniedException();
            using (var db = new CHDB())
            {
                db.PROBLEMs.Add(new PROBLEM()
                {
                    ID = Guid.NewGuid(),
                    Name = problem.Name,
                    Content = problem.Content,
                    Comparer = problem.Comparer,
                    CONTEST1 = (from c in db.CONTESTs
                                where c.Name == Name
                                select c).Single()
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除指定名称的题目
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="ContestNotStartedException"></exception>
        /// <exception cref="NotAttendedContestException"></exception>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="ProblemNotFoundException"></exception>
        public void RemoveProblem(string name)
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            if (!Owner.Contains(User.CurrentUser.name)
                && !User.CurrentUser.groups.Contains("Administrators"))
                throw new PermissionDeniedException();

            using (var db = new CHDB())
            {
                var prob = (from p in db.PROBLEMs
                            where p.Name == name && p.CONTEST1.Name == Name
                            select p).SingleOrDefault();
                if (null == prob)
                    throw new ProblemNotFoundException();
                db.PROBLEMs.Remove(prob);
                db.SaveChanges();
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Contest)
                return Name == ((Contest)obj).Name;
            return base.Equals(obj);
        }

        /// <summary>
        /// 返回ACM比赛Standing
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <param name="HasVirtual"></param>
        /// <returns></returns>
        /// <exception cref="ContestTypeMismatchException"></exception>
        public List<ACMStanding> GetACMStanding(int skip, int top,bool HasVirtual)
        {
            if (Type != ContestType.ACM)
                throw new ContestTypeMismatchException();
            using (var db = new CHDB())
            {
                var con = (from c in db.CONTESTs
                           where c.Name == Name
                           select c).Single();
                var result = (from u in con.CONTEST_ATTEND.Where(x => (x.Type != (int)AttendType.Practice && (HasVirtual ? true : x.Type != (int)AttendType.Virtual))).Select(x => x.USER1)
                              let des = from p in con.PROBLEMs
                                        orderby p.Name ascending
                                        let ACTimeList = (from r in p.RECORDs
                                                          where r.USER1 == u
                                                          && r.VirtualSubmitTime >= con.StartTime
                                                          && r.VirtualSubmitTime <= con.EndTime
                                                          && r.Status == (int)Record.StatusType.Accept
                                                          select r.VirtualSubmitTime)
                                        let ACTime = ACTimeList.Any() ? (DateTime?)ACTimeList.Min() : null
                                        let FailedTimes = (from r in p.RECORDs
                                                           where r.USER1 == u
                                                           && r.VirtualSubmitTime >= con.StartTime
                                                           && r.VirtualSubmitTime <= (ACTime == null ? con.EndTime : ACTime)
                                                           && r.Status > 0
                                                           select r).Count()
                                        select new ACMStanding.DescriptionClass()
                                        {
                                            ACTime = (null == ACTime) ? null : (int?)((DateTime)ACTime - con.StartTime).Minutes,
                                            isAC = null != ACTime,
                                            FailedTimes = FailedTimes
                                        }
                              select new ACMStanding
                              {
                                  User = u.Name,
                                  TotalTime = des.Sum(d => d.isAC ? d.FailedTimes * 20 + (int)d.ACTime : 0),
                                  Description = des.ToList(),
                                  CountAC = des.Sum(d => d.isAC ? 1 : 0),
                                  IsVirtual = u.CONTEST_ATTEND.Where(x => x.CONTEST1 == con).Single().Type == (int)AttendType.Virtual
                              });
                return result.OrderByDescending(s => s.CountAC).ThenBy(s => s.TotalTime).Skip(skip).Take(top).ToList();
            }
        }

        /// <summary>
        /// 返回OI比赛Standing
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <param name="HasVirtual"></param>
        /// <returns></returns>
        /// <exception cref="ContestTypeMismatchException"></exception>
        /// <exception cref="ContestNotEndedException"></exception>
        public List<OIStanding> GetOIStanding(int skip, int top,bool HasVirtual)
        {
            if (Type != ContestType.OI)
                throw new ContestTypeMismatchException();
            using (var db = new CHDB())
            {
                var con = (from c in db.CONTESTs
                           where c.Name == Name
                           select c).Single();
                if (DateTime.Now <= con.EndTime)
                    throw new ContestNotEndedException();
                return (from u in con.CONTEST_ATTEND.Where(x => (x.Type != (int)AttendType.Practice && (HasVirtual ? true : x.Type != (int)AttendType.Virtual))).Select(x => x.USER1)
                        let des = (from p in con.PROBLEMs
                                   orderby p.Name ascending
                                   let score = (from r in p.RECORDs
                                                where r.USER1 == u
                                                && r.PROBLEM1 == p
                                                && r.VirtualSubmitTime >= con.StartTime
                                                && r.VirtualSubmitTime <= con.EndTime
                                                orderby r.VirtualSubmitTime descending
                                                select r).FirstOrDefault()
                                   select score)
                        select new OIStanding
                        {
                            Scores = des.Select(x => (null == x ? null : x.Score)).ToList(),
                            TotalScore = des.Sum(x => (null == x ? 0 : (null == x.Score ? 0 : (int)x.Score))),
                            User = u.Name,
                            TotalTime = des.Sum(x => (null == x ? 0 : (null == x.ExecutedTime ? 0 : (int)x.ExecutedTime))),
                            IsVirtual = u.CONTEST_ATTEND.Where(x => x.CONTEST1 == con).Single().Type == (int)AttendType.Virtual
                        }).OrderByDescending(x => x.TotalScore).ThenBy(x => x.TotalTime).Skip(skip).Take(top).ToList();
            }
        }

        int CalcRating(int? ACTime, int totoalRating, int failedTimes, int succssfullyHunt, int unsuccessfullyHunt)
        {
            if (null == ACTime)
                return 0;
            int ratingbase = totoalRating * 3 / 5;
            int minRating = totoalRating * 2 / 5;
            int time = (int)ACTime;
            if (time > 30)
            {
                totoalRating -= ratingbase * 2 / 10;
                if (time > 60)
                {
                    totoalRating -= ratingbase * 4 / 10;
                    if (time > 90)
                    {
                        totoalRating -= ratingbase * 3 / 10;
                        totoalRating -= ratingbase * 1 / 10 * (time - 90) / 30;
                    }
                    else
                        totoalRating -= ratingbase * 3 / 10 * (time - 60) / 30;
                }
                else
                    totoalRating -= ratingbase * 4 / 10 * (time - 30) / 30;
            }
            else
                totoalRating -= ratingbase * 2 / 10 * time / 30;
            totoalRating -= failedTimes * 50;
            totoalRating += succssfullyHunt * 100;
            totoalRating -= unsuccessfullyHunt * 25;
            totoalRating = Math.Max(totoalRating, minRating);
            return totoalRating;
        }

        /// <summary>
        /// 返回CF比赛Standing
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <param name="HasVirtual"></param>
        /// <returns></returns>
        /// <exception cref="ContestTypeMismatchException"></exception>
        public List<CFStanding> GetCFStanding(int skip, int top, bool HasVirtual)
        {
            if (Type != ContestType.CF)
                throw new ContestTypeMismatchException();
            using (var db = new CHDB())
            {
                var con = (from c in db.CONTESTs
                           where c.Name == Name
                           select c).Single();
                var result = (from u in con.CONTEST_ATTEND.Where(x => (x.Type != (int)AttendType.Practice && (HasVirtual ? true : x.Type != (int)AttendType.Virtual))).Select(x => x.USER1)
                              let des = from p in con.PROBLEMs
                                        orderby p.Name ascending
                                        let ACTimeList = (from r in p.RECORDs
                                                          where r.USER1 == u
                                                          && r.VirtualSubmitTime >= con.StartTime
                                                          && r.VirtualSubmitTime <= con.EndTime
                                                          && r.Status == (int)Record.StatusType.Accept
                                                          select r.VirtualSubmitTime)
                                        let ACTime = ACTimeList.Any() ? (DateTime?)ACTimeList.Min() : null
                                        let FailedTimes = (from r in p.RECORDs
                                                           where r.USER1 == u
                                                           && r.VirtualSubmitTime >= con.StartTime
                                                           && r.VirtualSubmitTime <= (ACTime == null ? con.EndTime : ACTime)
                                                           && r.Status > 0
                                                           select r).Count()
                                        select new CFStanding.DescriptionClass()
                                        {
                                            ACTime = (null == ACTime) ? null : (int?)((DateTime)ACTime - con.StartTime).Minutes,
                                            isAC = null != ACTime,
                                            FailedTimes = FailedTimes,
                                            Rating = CalcRating((null == ACTime) ? null : (int?)((DateTime)ACTime - con.StartTime).Minutes,
                                                    (int)p.OriginRating, FailedTimes, 0, 0)
                                        }
                              select new CFStanding
                              {
                                  User = u.Name,
                                  TotalRating = des.Sum(x => x.Rating),
                                  FailedHack = 0,
                                  SeccessfullyHack = 0,
                                  IsVirtual=u.CONTEST_ATTEND.Where(x=>x.CONTEST1==con).Single().Type==(int)AttendType.Virtual
                              });
                return result.OrderByDescending(s=>s.TotalRating).Skip(skip).Take(top).ToList();

            }
        }

        /// <summary>
        /// 判断是否为虚拟报名此比赛
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="NotAttendedContestException"></exception>
        public AttendType GetAttendType()
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            if (!IsAttended())
                throw new NotAttendedContestException();
            using (var db = new CHDB())
            {
                var con_att = (from c in db.CONTESTs
                               where c.Name == Name
                               select c.CONTEST_ATTEND).Single();
                return (AttendType)(from u in con_att
                        where u.USER1.Name == User.CurrentUser.name
                        select u.Type).Single();
            }
        }

        /// <summary>
        /// 获得虚拟比赛开始时间
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="NotAttendedContestException"></exception>
        /// <exception cref="AttendedNotVirtualException"></exception>
        DateTime VirtualStartTime()
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            if (!IsAttended())
                throw new NotAttendedContestException();
            using (var db = new CHDB())
            {
                var con_atts = (from c in db.CONTESTs
                                where c.Name == Name
                                select c.CONTEST_ATTEND).Single();
                var con_att = (from u in con_atts
                               where u.USER1.Name == User.CurrentUser.name
                               select u).Single();
                if (con_att.Type != (int)AttendType.Virtual)
                    throw new AttendedNotVirtualException();
                return (DateTime)con_att.Time;
            }
        }

        /// <summary>
        /// 获得虚拟比赛结束时间
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="NotAttendedContestException"></exception>
        /// <exception cref="AttendedNotVirtualException"></exception>
        DateTime VirtualEndTime()
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            if (!IsAttended())
                throw new NotAttendedContestException();
            using (var db = new CHDB())
            {
                var con = (from c in db.CONTESTs
                           where c.Name == Name
                           select c).Single();
                var con_att = (from u in con.CONTEST_ATTEND
                               where u.USER1.Name == User.CurrentUser.name
                               select u).Single();
                if (con_att.Type != (int)AttendType.Virtual)
                    throw new AttendedNotVirtualException();
                return (DateTime)con_att.Time + (con.EndTime - con.StartTime);
            }
        }
    }
}