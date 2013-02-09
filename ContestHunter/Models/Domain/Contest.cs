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
        public ContestType Tp;
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
                             Tp = (ContestType)c.Type,
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
                            Tp = (ContestType)c.Type,
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
                            Tp = (ContestType)c.Type,
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
                    Type = (int)contest.Tp,
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

    }
}