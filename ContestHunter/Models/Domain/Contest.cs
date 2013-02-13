using System;
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
        public DateTime StartTime, EndTime;
        public string Description;
        public bool IsOfficial;
        public List<string> Owner;

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
                                ).OrderBy(x=>x.StartTime).Skip(skip).Take(top).ToList()
                        select new Contest
                         {
                             Name = c.Name,
                             Description = c.Description,
                             Type = (ContestType)c.Type,
                             StartTime = c.StartTime,
                             EndTime = c.EndTime,
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
                             where contest.StartTime <= DateTime.Now && contest.EndTime>=DateTime.Now
                             select contest
                                ).OrderBy(x=>x.StartTime).Skip(skip).Take(top).ToList()
                        select new Contest
                        {
                            Name = c.Name,
                            Description = c.Description,
                            Type = (ContestType)c.Type,
                            StartTime = c.StartTime,
                            EndTime = c.EndTime,
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
                            StartTime = c.StartTime,
                            EndTime = c.EndTime,
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
                    StartTime = contest.StartTime,
                    EndTime = contest.EndTime,
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
                              StartTime = c.StartTime,
                              EndTime = c.EndTime,
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
        /// <exception cref="ContestStartedException"></exception>
        public void Attend()
        {
            if(null == User.CurrentUser)
                throw new UserNotLoginException();
            if (IsAttended())
                throw new AlreadyAttendedContestException();
            if (DateTime.Now > StartTime)
                throw new ContestStartedException();
            using (var db = new CHDB())
            {
                (from c in db.CONTESTs
                 where c.Name == Name
                 select c).Single().ATTENDERs.Add(
                 (from u in db.USERs
                  where u.ID == User.CurrentUser.ID
                  select u).Single()
                  );
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
            if (DateTime.Now > StartTime)
                throw new ContestStartedException();
            using (var db = new CHDB())
            {
                (from c in db.CONTESTs
                 where c.Name == Name
                 select c).Single().ATTENDERs.Remove(
                 (from u in db.USERs
                  where u.ID == User.CurrentUser.ID
                  select u).Single()
                  );
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 判断是否报名比赛
        /// </summary>
        /// <returns></returns>
        public bool IsAttended()
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            using (var db = new CHDB())
            {
                return (from u in
                            (from c in db.CONTESTs
                             where c.Name == Name
                             select c).Single().ATTENDERs
                        where u.Name == User.CurrentUser.name
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
                        select c.ATTENDERs).Single().Count();
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
                             select c.ATTENDERs).Single()
                        select u.Name).OrderBy(u =>u).Skip(skip).Take(top).ToList();

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
                        where p.CONTEST1.Name==Name
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
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            if (!Owner.Contains(User.CurrentUser.name) && !User.CurrentUser.groups.Contains("Administrators"))
            {
                if (DateTime.Now < StartTime)
                    throw new ContestNotStartedException();
                if (DateTime.Now <= EndTime && !IsAttended())
                    throw new NotAttendedContestException();
            }
            using (var db = new CHDB())
            {
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
                                  Contest=result.CONTEST1.Name,
                                  Owner = new List<string>(Owner)
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
        /// <returns></returns>
        /// <exception cref="ContestTypeMismatchException"></exception>
        public List<ACMStanding> GetACMStanding(int skip, int top)
        {
            if (Type != ContestType.ACM)
                throw new ContestTypeMismatchException();
            using (var db = new CHDB())
            {
                var con = (from c in db.CONTESTs
                           where c.Name == Name
                           select c).Single();
                var result = (from u in con.ATTENDERs
                              let des = from p in con.PROBLEMs
                                        orderby p.Name ascending
                                        let ACTimeList = (from r in p.RECORDs
                                                          where r.USER1 == u
                                                          && r.SubmitTime >= con.StartTime
                                                          && r.SubmitTime <= con.EndTime
                                                          && r.Status == (int)Record.StatusType.Accept
                                                          select r.SubmitTime)
                                        let ACTime = ACTimeList.Any() ? (DateTime?)ACTimeList.Min() : null
                                        let FailedTimes = (from r in p.RECORDs
                                                           where r.USER1 == u
                                                           && r.SubmitTime >= con.StartTime
                                                           && r.SubmitTime <= (ACTime == null ? con.EndTime : ACTime)
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
                                  CountAC = des.Sum(d => d.isAC ? 1 : 0)
                              });
                return result.OrderByDescending(s => s.CountAC).ThenBy(s => s.TotalTime).Skip(skip).Take(top).ToList();
            }
        }

        /// <summary>
        /// 返回OI比赛Standing
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        /// <exception cref="ContestTypeMismatchException"></exception>
        /// <exception cref="ContestNotEndedException"></exception>
        public List<OIStanding> GetOIStanding(int skip, int top)
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
                return (from u in con.ATTENDERs
                        let des = (from p in con.PROBLEMs
                                   orderby p.Name ascending
                                   let score = (from r in p.RECORDs
                                                where r.USER1 == u
                                                && r.PROBLEM1 == p
                                                && r.SubmitTime >= con.StartTime
                                                && r.SubmitTime <= con.EndTime
                                                orderby r.SubmitTime descending
                                                select r).FirstOrDefault()
                                   select score)
                        select new OIStanding
                        {
                            Scores = des.Select(x => (null == x ? null : x.Score)).ToList(),
                            TotalScore = des.Sum(x => (null == x ? 0 : (int)x.Score)),
                            User = u.Name,
                            TotalTime = des.Sum(x => (null == x ? 0 : (null == x.ExecutedTime ? 0 : (int)x.ExecutedTime)))
                        }).OrderByDescending(x => x.TotalScore).ThenBy(x => x.TotalTime).Skip(skip).Take(top).ToList();
            }
        }


    }
}