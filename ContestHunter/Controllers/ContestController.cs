using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using ContestHunter.Models.Domain;
using ContestHunter.Models.View;
using USER = ContestHunter.Models.Domain.User;
namespace ContestHunter.Controllers
{
    public class ContestController : Controller
    {
        static readonly int INDEX_PAGE_SIZE = int.Parse(ConfigurationManager.AppSettings["Contest.Index.PageSize"]);
        static readonly int STANDING_PAGE_SIZE = int.Parse(ConfigurationManager.AppSettings["Contest.Standing.PageSize"]);

        [AllowAnonymous]
        public ActionResult Index(ContestListModel model)
        {
            model.Pending = Contest.Pending(model.PendingPageIndex * INDEX_PAGE_SIZE, INDEX_PAGE_SIZE);
            model.Testing = Contest.Testing(model.TestingPageIndex * INDEX_PAGE_SIZE, INDEX_PAGE_SIZE);
            model.Done = Contest.Done(model.DonePageIndex * INDEX_PAGE_SIZE, INDEX_PAGE_SIZE);

            model.PendingPageCount = (int)Math.Ceiling(Contest.PendingCount() / (double)INDEX_PAGE_SIZE);
            model.TestingPageCount = (int)Math.Ceiling(Contest.TestingCount() / (double)INDEX_PAGE_SIZE);
            model.DonePageCount = (int)Math.Ceiling(Contest.DoneCount() / (double)INDEX_PAGE_SIZE);

            return View(model);
        }

        public ActionResult Show(string id)
        {
            Contest contest;
            try
            {
                contest = Contest.ByName(id);
                ViewBag.Problems = contest.Problems();
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有相应的比赛" });
            }
            catch (UserNotLoginException)
            {
                throw;
            }
            catch (NotAttendedContestException)
            {
                return RedirectToAction("Signup", new { id = id });
            }
            return View(contest);
        }

        public ActionResult Standing(string id, ContestStandingModel model)
        {
            try
            {
                Contest contest = Contest.ByName(id);
                model.Problems = contest.Problems();
                model.PageCount = (int)Math.Ceiling(contest.AttendedUsersCount() / (double)STANDING_PAGE_SIZE);
                model.Contest = contest;
                model.StartIndex = model.PageIndex * STANDING_PAGE_SIZE;

                switch (contest.Type)
                {
                    case Contest.ContestType.ACM:
                        model.ACM = contest.GetACMStanding(model.StartIndex, STANDING_PAGE_SIZE, model.ShowVirtual, model.ShowNoSubmit);
                        break;
                    case Contest.ContestType.OI:
                        model.OI = contest.GetOIStanding(model.StartIndex, STANDING_PAGE_SIZE, model.ShowVirtual, model.ShowNoSubmit);
                        break;
                    case Contest.ContestType.CF:
                        model.CF = contest.GetCFStanding(model.StartIndex, STANDING_PAGE_SIZE, model.ShowVirtual, model.ShowNoSubmit);
                        break;
                }
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "比赛不存在" });
            }
            catch (ContestTypeMismatchException)
            {
                throw;
            }
            catch (ContestNotEndedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "比赛尚未结束，不能查看排名" });
            }
            catch (NotAttendedContestException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "您没有参与比赛，不能查看排名" });
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult Signup(string id)
        {
            Contest contest;
            try
            {
                contest = Contest.ByName(id);
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有相应的比赛" });
            }

            ContestSignupModel model = new ContestSignupModel
            {
                Contest = contest,
                StartTime = DateTime.Now.AddMinutes(5)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Signup(string id, ContestSignupModel model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                switch (model.Type.Value)
                {
                    case ContestSignupModel.SignupType.Attend:
                        Contest.ByName(id).Attend();
                        break;
                    case ContestSignupModel.SignupType.Virtual:
                        if (model.StartTime == null)
                        {
                            ModelState.AddModelError("StartTime", "开始时间非法");
                            return View(model);
                        }
                        Contest.ByName(id).VirtualAttend(model.StartTime.Value);
                        break;
                    case ContestSignupModel.SignupType.Practice:
                        Contest.ByName(id).PracticeAttend();
                        break;
                    case ContestSignupModel.SignupType.Cancel:
                        Contest.ByName(id).Disattended();
                        break;
                }
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有这场比赛" });
            }
            catch (UserNotLoginException)
            {
                throw;
            }
            catch (AlreadyAttendedContestException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "不可重复参加比赛" });
            }
            catch (ContestEndedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "比赛已结束，不可报名" });
            }
            catch (ContestNotStartedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "比赛尚未开始" });
            }
            catch (ContestStartedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "比赛已经开始，不可取消参赛" });
            }
            catch (VirtualStartTooEarlyException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "模拟比赛开始时间不得早于当前时间" });
            }

            return RedirectToAction("Show", new { id = id });
        }

        [HttpGet]
        public ActionResult Add()
        {
            ContestBasicInfoModel model = new ContestBasicInfoModel
            {
                Owner1 = USER.CurrentUserName,
                StartTime = DateTime.Now,
                Hour = 0,
                Minute = 0,
                Weight = 16
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(ContestBasicInfoModel model)
        {
            if (!ModelState.IsValid) return View(model);
            List<string> owners = new List<string>();
            owners.Add(USER.CurrentUserName);
            if (model.Owner2 != null)
                owners.Add(model.Owner2);
            if (model.Owner3 != null)
                owners.Add(model.Owner3);
            try
            {
                Contest.Add(new Contest
                {
                    AbsoluteStartTime = model.StartTime.Value,
                    AbsoluteEndTime = model.StartTime.Value + new TimeSpan(model.Hour.Value, model.Minute.Value, 0),
                    Description = "",
                    IsOfficial = model.IsOfficial,
                    Name = model.Name,
                    Type = model.Type.Value,
                    Owners = owners,
                    Weight = model.Weight
                });
            }
            catch (UserNotLoginException)
            {
                throw;
            }
            catch (PermissionDeniedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有创建比赛的相关权限" });
            }
            catch (ContestNameExistedException)
            {
                ModelState.AddModelError("Name", "比赛名称已存在");
                return View(model);
            }

            return RedirectToAction("Preview", new { id = model.Name });
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            ContestBasicInfoModel model = new ContestBasicInfoModel
            {
                Contest = id,
            };
            try
            {
                Contest con = Contest.ByName(id);
                model.Hour = (int)(con.AbsoluteEndTime - con.AbsoluteStartTime).TotalHours;
                model.Minute = (con.AbsoluteEndTime - con.AbsoluteStartTime).Minutes;
                model.IsOfficial = con.IsOfficial;
                model.Name = con.Name;
                if (USER.CurrentUserIsAdmin)
                {
                    model.Owner1 = USER.CurrentUserName;
                }
                else
                {
                    model.Owner1 = USER.CurrentUserName;
                }
                var otherOwners = con.Owners.Where(u => u != USER.CurrentUserName);
                model.Owner2 = otherOwners.FirstOrDefault();
                model.Owner3 = otherOwners.Skip(1).FirstOrDefault();
                model.StartTime = con.AbsoluteStartTime;
                model.Type = con.Type;
                model.Weight = con.Weight;
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "找不到相应比赛" });
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, ContestBasicInfoModel model)
        {
            if (!ModelState.IsValid) return View(model);
            List<string> owners = new List<string>();
            owners.Add(USER.CurrentUserName);
            if (model.Owner2 != null)
                owners.Add(model.Owner2);
            if (model.Owner3 != null)
                owners.Add(model.Owner3);
            try
            {
                Contest con = Contest.ByName(id);
                con.AbsoluteStartTime = model.StartTime.Value;
                con.AbsoluteEndTime = model.StartTime.Value + new TimeSpan(model.Hour.Value, model.Minute.Value, 0);
                con.IsOfficial = model.IsOfficial;
                con.Name = model.Name;
                con.Type = model.Type.Value;
                con.Owners = owners;
                con.Weight = model.Weight;
                con.Change();
                model.Name = con.Name;
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "找不到相应比赛" });
            }
            catch (UserNotLoginException)
            {
                throw;
            }
            catch (PermissionDeniedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有修改比赛信息的相关权限" });
            }
            catch (ContestNameExistedException)
            {
                ModelState.AddModelError("Name", "比赛名称已存在");
                return View(model);
            }

            return RedirectToAction("Preview", new { id = model.Name });
        }

        [HttpGet]
        public ActionResult Preview(string id)
        {
            ContestDescriptionModel model = new ContestDescriptionModel
            {
                Contest = id
            };
            try
            {
                Contest contest = Contest.ByName(id);
                model.Description = contest.Description;
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有找到相关比赛" });
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Preview(string id, ContestDescriptionModel model)
        {
            if (!ModelState.IsValid) return View(model);
            switch (model.Action)
            {
                case ContestDescriptionModel.ActionType.Preview:
                    return View(model);
                case ContestDescriptionModel.ActionType.Next:
                    try
                    {
                        Contest contest = Contest.ByName(id);
                        contest.Description = model.Description;
                        contest.Change();
                    }
                    catch (ContestNotFoundException)
                    {
                        return RedirectToAction("Error", "Shared", new { msg = "没有找到相关比赛" });
                    }
                    catch (UserNotLoginException)
                    {
                        throw;
                    }
                    catch (PermissionDeniedException)
                    {
                        return RedirectToAction("Error", "Shared", new { msg = "没有修改比赛预告的相关权限" });
                    }
                    catch (ContestNameExistedException)
                    {
                        throw;
                    }
                    return RedirectToAction("Problems", new { id = id });
            }
            throw new NotImplementedException();
        }

        [HttpGet]
        public ActionResult Problems(string id)
        {
            ContestProblemsModel model = new ContestProblemsModel
            {
                Contest = id
            };
            try
            {
                Contest contest = Contest.ByName(id);
                model.Problems = (from p in contest.Problems().Select(contest.ProblemByName)
                                  select new ProblemInfo
                                  {
                                      Name = p.Name,
                                      Owner = p.Owner,
                                      TestCaseCount = p.TestCases().Count
                                  }).ToList();
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有找到相关比赛" });
            }
            catch (ProblemNotFoundException)
            {
                throw;
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Problems(string id, ContestProblemsModel model)
        {
            if (!ModelState.IsValid) return View(model);
            ModelState.Clear();
            try
            {
                Contest contest = Contest.ByName(id);
                contest.RemoveProblem(model.Problems[model.ProblemIndex].Name);
                model.Problems.RemoveAt(model.ProblemIndex);
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有找到相关比赛" });
            }
            catch (UserNotLoginException)
            {
                throw;
            }
            catch (ProblemNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有找到相关题目" });
            }

            return View(model);
        }

        public ActionResult Complete(string id)
        {
            ViewBag.Contest = id;
            return View();
        }

        public ActionResult Mine()
        {
            MyContestModel model = new MyContestModel();
            model.Contests = Contest.ByOwner(USER.CurrentUserName).Select(c => new MyContestModel.ContestInfo
            {
                AttendUserCount = c.AttendedUsersCount(),
                Name = c.Name,
                OtherOwners = c.Owners.Where(o => o != USER.CurrentUserName).ToList(),
                StartTime = c.AbsoluteStartTime
            }).ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReCalcRating(string id)
        {
            try
            {
                Contest.ByName(id).ReCalcRating();
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有这场比赛" });
            }
            catch (UserNotLoginException)
            {
                throw;
            }
            catch (PermissionDeniedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "您没有权限重算Rating" });
            }
            return RedirectToAction("Show", new { id = id });
        }

        [HttpGet]
        public ActionResult SendInviteEmail(string id)
        {
            ContestInviteEmailModel model = new ContestInviteEmailModel() { Contest = id };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendInviteEmail(string id,ContestInviteEmailModel model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                Contest.ByName(id).SendEamil(model.Content);
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有这场比赛" });
            }
            catch (UserNotLoginException)
            {
                throw;
            }
            catch (PermissionDeniedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "您没有权限重算Rating" });
            }
            return RedirectToAction("Complete", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            try
            {
                Contest.ByName(id).Remove();
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有这场比赛" });
            }
            catch (UserNotLoginException)
            {
                throw;
            }
            catch (PermissionDeniedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "您没有权限删除比赛" });
            }
            return RedirectToAction("Index");
        }
    }
}
