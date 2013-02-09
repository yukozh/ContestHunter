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
        public static Contest ByName(string name)
        {
            using (var db = new CHDB())
            {
                return (from c in
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
                    }).Single();
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

    }
}