using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContestHunter.Database;
using System.Threading.Tasks;
using System.Configuration;

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
        internal DateTime RelativeNow
        {
            get
            {
                return AbsoluteStartTime + (DateTime.Now - RelativeStartTime);
            }
        }
        public string Description;
        public bool IsOfficial;
        internal List<string> Owner;
        public List<string> Owners;
        public int Weight;

        public enum AttendType
        {
            Normal,
            Virtual,
            Practice
        }

        internal enum StatusType
        {
            BeforeFinalTest,
            FinalTesting,
            Done
        }

        internal Guid ID;

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
                var cons = (from contest in db.CONTESTs
                            where contest.StartTime > DateTime.Now
                            select contest
                                ).OrderBy(x => x.StartTime).Skip(skip).Take(top).ToList();
                List<Contest> Ret = new List<Contest>();
                foreach (var c in cons)
                {
                    var Own = (from u in c.OWNERs
                               select u.Name).ToList();
                    Ret.Add(new Contest
                            {
                                ID = c.ID,
                                Name = c.Name,
                                Description = c.Description,
                                Type = (ContestType)c.Type,
                                RelativeStartTime = c.StartTime,
                                RelativeEndTime = c.EndTime,
                                IsOfficial = c.IsOfficial,
                                Owner = Own,
                                Owners = Own
                            });
                }
                return Ret;
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
                var cons = (from contest in db.CONTESTs
                            where contest.StartTime <= DateTime.Now && contest.EndTime >= DateTime.Now
                            select contest
                                ).OrderBy(x => x.StartTime).Skip(skip).Take(top).ToList();
                List<Contest> Ret=new List<Contest>();
                foreach (var c in cons)
                {
                    var Own = (from u in c.OWNERs
                               select u.Name).ToList();
                    Ret.Add(new Contest
                            {
                                ID = c.ID,
                                Name = c.Name,
                                Description = c.Description,
                                Type = (ContestType)c.Type,
                                RelativeStartTime = c.StartTime,
                                RelativeEndTime = c.EndTime,
                                IsOfficial = c.IsOfficial,
                                Owner = Own,
                                Owners = Own
                            });
                }
                return Ret;
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
                var cons = (from contest in db.CONTESTs
                            where contest.EndTime < DateTime.Now
                            orderby contest.StartTime descending
                            select contest
                                ).Skip(skip).Take(top).ToList();
                List<Contest> Ret=new List<Contest>();
                foreach (var c in cons)
                {
                    var Own = (from u in c.OWNERs
                               select u.Name).ToList();
                    Ret.Add(new Contest
                            {
                                ID = c.ID,
                                Name = c.Name,
                                Description = c.Description,
                                Type = (ContestType)c.Type,
                                RelativeStartTime = c.StartTime,
                                RelativeEndTime = c.EndTime,
                                IsOfficial = c.IsOfficial,
                                Owner = Own,
                                Owners = Own
                            });
                }
                return Ret;
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
        /// <exception cref="PermissionDeniedException"></exception>
        /// <exception cref="ContestNameExistedException"></exception>
        public static void Add(Contest contest)
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            if (!contest.Owners.Contains(User.CurrentUser.name))
                throw new PermissionDeniedException();
            using (var db = new CHDB())
            {
                if (contest.IsOfficial && !User.CurrentUser.IsAdmin
                    && User.ByName(User.CurrentUser.name).Rating() < 2100)
                    throw new PermissionDeniedException();
                contest.Name = Helper.GetLegalName(contest.Name);
                if ((from c in db.CONTESTs
                     where c.Name == contest.Name
                     select c).Any())
                    throw new ContestNameExistedException();
                var curContest = db.CONTESTs.Add(new CONTEST()
                {
                    ID = Guid.NewGuid(),
                    Name = Helper.GetLegalName(contest.Name),
                    StartTime = contest.AbsoluteStartTime,
                    EndTime = contest.AbsoluteEndTime,
                    Description = contest.Description,
                    Type = (int)contest.Type,
                    IsOfficial = contest.IsOfficial,
                    Weight = User.CurrentUser.IsAdmin ? contest.Weight : 16
                });

                foreach (string name in contest.Owners)
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
        /// 修改Contest信息
        /// </summary>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="PermissionDeniedException"></exception>
        /// <exception cref="ContestNameExistedException"></exception>
        public void Change()
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            if (!Owner.Contains(User.CurrentUser.name) &&
                !User.CurrentUser.IsAdmin)
                throw new PermissionDeniedException();
            using (var db = new CHDB())
            {
                var con = (from c in db.CONTESTs
                           where c.ID==ID
                           select c).Single();
                if (IsOfficial != con.IsOfficial && !User.CurrentUser.IsAdmin &&
                    User.ByName(User.CurrentUser.name).Rating() < 2100)
                    throw new PermissionDeniedException();
                Name = Helper.GetLegalName(Name);
                if (con.Name != Helper.GetLegalName(Name))
                {
                    if ((from c in db.CONTESTs
                         where c.Name == Name
                         select c).Any())
                        throw new ContestNameExistedException();
                    con.Name = Name;
                }
                con.Description = Description;
                if (Owner != Owners)
                {
                    con.OWNERs.Clear();
                    foreach (var name in Owners)
                    {
                        con.OWNERs.Add((from u in db.USERs
                                        where u.Name == name
                                        select u).Single());
                    }
                }
                con.IsOfficial = IsOfficial;
                con.StartTime = AbsoluteStartTime;
                con.EndTime = AbsoluteEndTime;
                con.Type = (int)Type;
                if (User.CurrentUser.IsAdmin)
                    con.Weight = Weight;
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
                var con=(from c in db.CONTESTs
                                   where c.Name == name
                                   select c).SingleOrDefault();
                if(null==con)
                    throw new ContestNotFoundException();
                var Own=con.OWNERs.Select(x => x.Name).ToList();
                return new Contest
                {
                    Name = con.Name,
                    Description = con.Description,
                    RelativeStartTime = con.StartTime,
                    RelativeEndTime = con.EndTime,
                    IsOfficial = con.IsOfficial,
                    Type = (ContestType)con.Type,
                    Owner = Own,
                    Owners = Own,
                    ID = con.ID,
                    Weight = con.Weight
                };
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
                           where c.ID==ID
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
                           where c.ID==ID
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
        /// 报名练习比赛
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
                           where c.ID==ID
                           select c).Single();
                con.CONTEST_ATTEND.Add(new CONTEST_ATTEND()
                {
                    USER1 = (from u in db.USERs
                             where u.ID == User.CurrentUser.ID
                             select u).Single(),
                    Type = (int)AttendType.Practice,
                    Time = con.StartTime
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
                           where c.ID==ID
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
                             where c.ID==ID
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
                        where c.ID==ID
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
                             where c.ID==ID
                             select c.CONTEST_ATTEND).Single()
                        orderby u.USER1.Name ascending
                        select u.USER1.Name).Skip(skip).Take(top).ToList();

            }
        }

        /// <summary>
        /// 获取比赛题目名称列表
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="NotAttendedContestException"></exception>
        public List<string> Problems()
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            if (!Owner.Contains(User.CurrentUser.name) && !User.CurrentUser.IsAdmin)
            {
                if (!IsAttended())
                    throw new NotAttendedContestException();
            }
   
            using (var db = new CHDB())
            {

                return (from p in db.PROBLEMs
                        where p.CONTEST1.ID==ID
                        orderby p.OriginRating,p.Name ascending
                        select p.Name).ToList();
            }
        }

        /// <summary>
        /// 获得相应比赛的指定名称的题目
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ProblemNotFoundException"></exception>
        public Problem ProblemByName(string name)
        {
            bool privillege = true;
            bool Admin = false;
            using (var db = new CHDB())
            {
                if (null == User.CurrentUser)
                {
                    privillege = false;
                }
                else
                {
                    if (!Owner.Contains(User.CurrentUser.name) && !User.CurrentUser.IsAdmin)
                    {
                        var con = (from c in db.CONTESTs
                                   where c.ID == ID
                                   select c).Single();
                        if (!IsAttended())
                        {
                            privillege = false;
                        }
                        else
                        {
                            if (DateTime.Now < RelativeStartTime)
                                privillege = false;
                        }
                    }
                    else
                        Admin = true;
                }
                var result = (from p in db.PROBLEMs
                              where p.Name == name && p.CONTEST1.ID==ID
                              select p).SingleOrDefault();
                if (null == result)
                    throw new ProblemNotFoundException();

                return new Problem()
                              {
                                  Name = result.Name,
                                  Content = privillege ? result.Content : null,
                                  Comparer = Admin ? result.Comparer : null,
                                  ID = result.ID,
                                  Contest = result.CONTEST1.Name,
                                  OriginRating = result.OriginRating,
                                  DataChecker = Admin ? result.DataChecker : null,
                                  DataCheckerLanguage = Admin ? ((Record.LanguageType?)result.DataCheckerLanguage) : null,
                                  ComparerLanguage = Admin ? ((Record.LanguageType?)result.ComparerLanguage) : null,
                                  Owner = result.OWNER.Name,
                                  contest = this,
                                  privillege = privillege
                              };
            }

        }

        /// <summary>
        /// 添加题目 所有赛制必须填充 Name,Content,Comparer,Owner,DataChecker CF赛制还须填充OriginRating
        /// </summary>
        /// <param name="problem"></param>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="PermissionDeniedException"></exception>
        /// <exception cref="ProblemNameExistedException"></exception>
        /// <exception cref="UserNotFoundException"></exception>
        public void AddProblem(Problem problem)
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            if (!Owner.Contains(User.CurrentUser.name)
                && !User.CurrentUser.IsAdmin)
                throw new PermissionDeniedException();
            using (var db = new CHDB())
            {
                problem.Name = Helper.GetLegalName(problem.Name);
                if ((from p in db.PROBLEMs
                     where p.Name == problem.Name && p.CONTEST1.ID==ID
                     select p).Any())
                    throw new ProblemNameExistedException();
                var owner = (from u in db.USERs
                             where u.Name == problem.Owner
                             select u).SingleOrDefault();
                if (null == owner)
                    throw new UserNotFoundException();
                db.PROBLEMs.Add(new PROBLEM()
                {
                    ID = Guid.NewGuid(),
                    Name = problem.Name,
                    Content = problem.Content,
                    Comparer = problem.Comparer,
                    ComparerLanguage = (int)problem.ComparerLanguage,
                    OriginRating = problem.OriginRating,
                    DataChecker = problem.DataChecker,
                    DataCheckerLanguage = (int)problem.DataCheckerLanguage,
                    CONTEST1 = (from c in db.CONTESTs
                                where c.ID == ID
                                select c).Single(),
                    OWNER = owner
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除指定名称的题目
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="ContestNotStartedException"></exception>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="ProblemNotFoundException"></exception>
        public void RemoveProblem(string name)
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            if (!Owner.Contains(User.CurrentUser.name)
                && !User.CurrentUser.IsAdmin)
                throw new PermissionDeniedException();

            using (var db = new CHDB())
            {
                var prob = (from p in db.PROBLEMs
                            where p.Name == name && p.CONTEST1.ID==ID
                            select p).SingleOrDefault();
                if (null == prob)
                    throw new ProblemNotFoundException();
                db.PROBLEMs.Remove(prob);
                db.SaveChanges();
            }
        }

        /*
        public override bool Equals(object obj)
        {
            if (obj is Contest)
                return Name == ((Contest)obj).Name;
            return base.Equals(obj);
        }
        */
        /// <summary>
        /// 返回ACM比赛Standing
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <param name="HasVirtual"></param>
        /// <returns></returns>
        /// <exception cref="ContestTypeMismatchException"></exception>
        public List<ACMStanding> GetACMStanding(int skip, int top,bool HasVirtual,bool HasNotSubmit)
        {
            if (Type != ContestType.ACM)
                throw new ContestTypeMismatchException();
            using (var db = new CHDB())
            {
                var con = (from c in db.CONTESTs
                           where c.ID==ID
                           select c).Single();
                var result = (from u in con.CONTEST_ATTEND.Where(x => (x.Type != (int)AttendType.Practice && (HasVirtual ? true : x.Type != (int)AttendType.Virtual))).Select(x => x.USER1)
                              where HasNotSubmit?true:(from r in db.RECORDs
                                                       where r.USER1.ID == u.ID
                                                       && r.PROBLEM1.CONTEST1.ID == con.ID
                                                       select r).Any()
                              let des = from p in con.PROBLEMs
                                        orderby p.OriginRating,p.Name ascending
                                        let ACTimeList = (from r in p.RECORDs
                                                          where r.USER1 == u
                                                          && r.VirtualSubmitTime >= con.StartTime
                                                          && r.VirtualSubmitTime <= (con.EndTime < RelativeNow ? con.EndTime : RelativeNow)
                                                          && r.Status == (int)Record.StatusType.Accept
                                                          select r.VirtualSubmitTime)
                                        let ACTime = ACTimeList.Any() ? (DateTime?)ACTimeList.Min() : null
                                        let FailedTimes = (from r in p.RECORDs
                                                           where r.USER1 == u
                                                           && r.VirtualSubmitTime >= con.StartTime
                                                           && r.VirtualSubmitTime <= (ACTime == null ? (con.EndTime < RelativeNow ? con.EndTime : RelativeNow) : ACTime)
                                                           && r.Status > 0
                                                           select r).Count()
                                        select new ACMStanding.DescriptionClass()
                                        {
                                            ACTime = (null == ACTime) ? null : (int?)((DateTime)ACTime - con.StartTime).TotalMinutes,
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
        public List<OIStanding> GetOIStanding(int skip, int top,bool HasVirtual,bool HasNotSubmit)
        {
            if (Type != ContestType.OI)
                throw new ContestTypeMismatchException();
            using (var db = new CHDB())
            {
                var con = (from c in db.CONTESTs
                           where c.ID==ID
                           select c).Single();
                if (DateTime.Now <= RelativeEndTime && !User.CurrentUser.IsAdmin && !Owner.Contains(User.CurrentUserName))
                    throw new ContestNotEndedException();
                return (from u in con.CONTEST_ATTEND.Where(x => (x.Type != (int)AttendType.Practice && (HasVirtual ? true : x.Type != (int)AttendType.Virtual))).Select(x => x.USER1)
                        where HasNotSubmit ? true : (from r in db.RECORDs
                                                     where r.USER1.ID == u.ID
                                                     && r.PROBLEM1.CONTEST1.ID == con.ID
                                                     select r).Any()
                        let des = (from p in con.PROBLEMs
                                   orderby p.OriginRating,p.Name ascending
                                   let score = (from r in p.RECORDs
                                                where r.USER1 == u
                                                && r.PROBLEM1 == p
                                                && r.VirtualSubmitTime >= con.StartTime
                                                && r.VirtualSubmitTime <= (con.EndTime < RelativeNow ? con.EndTime:RelativeNow)
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

        int CalcRating(int? ACTime, int totoalRating, int failedTimes)
        {
            if (null == ACTime)
            {
                totoalRating = 0;
            }
            else
            {
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
                totoalRating = Math.Max(totoalRating, minRating);
            }
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
        public List<CFStanding> GetCFStanding(int skip, int top, bool HasVirtual,bool HasNotSubmit)
        {
            if (Type != ContestType.CF)
                throw new ContestTypeMismatchException();
            List<CFStanding> ret=new List<CFStanding>();
            var Probs=Problems();
            using (var db = new CHDB())
            {
                var raw = db.GetCFStanding(ID, RelativeNow,skip, top, HasVirtual, HasNotSubmit).ToArray();

                for (int i = 0; i < raw.Length; i += Probs.Count())
                {
                    var desp = new List<CFStanding.DescriptionClass>();
                    for (int jj = 0; jj < Probs.Count(); jj++)
                    {
                        int j = i + jj;
                        desp.Add(new CFStanding.DescriptionClass()
                        {
                            _huntFailed=(int)raw[j].FailedHunt,
                            _huntSuccessfully=(int)raw[j].SuccessfulHunt,
                            ACTime=raw[j].ACTime,
                            isAC=(bool)raw[j].IsAC,
                            FailedTimes=(int)raw[j].FailedTimes,
                            Rating=(int)raw[j].Rating
                        });
                    }
                    var entity = new CFStanding()
                    {
                        Description = desp,
                        FailedHack = desp.Sum(x => x._huntFailed),
                        SuccessHack = desp.Sum(x => x._huntSuccessfully),
                        User = raw[i].User,
                        TotalRating = desp.Sum(x => x.Rating),
                        IsVirtual = raw[i].Type == (int)AttendType.Virtual
                    };
                    entity.TotalRating = entity.TotalRating + 100 * entity.SuccessHack - 25 * entity.FailedHack;
                    ret.Add(entity);
                }
                
            }
            return ret;
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
                               where c.ID==ID
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
                                where c.ID==ID
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
                           where c.ID==ID
                           select c).Single();
                var con_att = (from u in con.CONTEST_ATTEND
                               where u.USER1.Name == User.CurrentUser.name
                               select u).Single();
                if (con_att.Type != (int)AttendType.Virtual)
                    throw new AttendedNotVirtualException();
                return (DateTime)con_att.Time + (con.EndTime - con.StartTime);
            }
        }

        /// <summary>
        /// 获得指定用户所创建的所有比赛
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static List<Contest> ByOwner(string name)
        {
            using (var db = new CHDB())
            {
                var cons = (from c in db.CONTESTs
                            where c.OWNERs.Select(x => x.Name).Contains(name)
                            select c);
                List<Contest> Ret = new List<Contest>();
                foreach (var con in cons)
                {
                    var Own = con.OWNERs.Select(x => x.Name).ToList();
                    Ret.Add(new Contest
                    {
                        Name = con.Name,
                        Description = con.Description,
                        RelativeStartTime = con.StartTime,
                        RelativeEndTime = con.EndTime,
                        IsOfficial = con.IsOfficial,
                        Type = (ContestType)con.Type,
                        Owner = Own,
                        Owners = Own,
                        ID = con.ID
                    });
                }
                return Ret;
            }
        }

        /// <summary>
        /// 重新计算Rating
        /// </summary>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="PermissionDeniedException"></exception>
        public void ReCalcRating()
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            if (!User.CurrentUser.IsAdmin && !Owner.Contains(User.CurrentUserName))
                throw new PermissionDeniedException();
            using (var db = new CHDB())
            {

                foreach (var r in (from r in db.RATINGs
                                   where r.CONTEST1.ID == ID
                                   select r))
                    db.RATINGs.Remove(r);
                (from c in db.CONTESTs
                 where c.ID == ID
                 select c).Single().Status = (int)StatusType.BeforeFinalTest;
                db.SaveChanges();
            }
        }

        public void SendEamil(string message)
        {
            if(null == User.CurrentUser)
                throw new UserNotLoginException();
            if(!User.CurrentUser.IsAdmin && !Owner.Contains(User.CurrentUserName))
                throw new PermissionDeniedException();
            using (var db = new CHDB())
            {
                var Info = (from u in db.USERs
                          where u.AcceptEmail
                          select new 
                          {
                              u.Name,
                              u.Email
                          }).ToArray();
                string Subject = "ContestHunter将举办 " + Name + " ，欢迎参加！";
                string link = ConfigurationManager.AppSettings["WebSite"] + "Contest/Show/" + Name;
                Parallel.ForEach(Info, info =>
                {
                    try
                    {
                        string Body = info.Name + "：<br />&nbsp;&nbsp;&nbsp;&nbsp;您好。" + HttpUtility.HtmlEncode(Name) + " 将于 " + AbsoluteStartTime + " 在ContestHunter举办。欢迎您访问 <a href=\"" + HttpUtility.HtmlAttributeEncode(link) + "\">" + HttpUtility.HtmlEncode(link) + "</a> 报名参加。<br />" +
                            "本次比赛持续 " + (AbsoluteEndTime - AbsoluteStartTime) + " ，采用 " + Type + " 赛制，由 " + string.Join(",", Owner) + " 举办，<b>" + (IsOfficial ? "" : "不") + "计入</b>能力排名。<br />" +
                            "诚邀您及时报名参加本场比赛。如果您不希望再收到此类邮件，可以在修改个人资料页面取消。<br />" +
                            (string.IsNullOrWhiteSpace(message) ? "" : "管理员留言：<br />" + HttpUtility.HtmlEncode(message));
                        EmailHelper.Send(Subject, info.Email, Body);
                    }
                    catch { }
                });
            }
        }
    }
}