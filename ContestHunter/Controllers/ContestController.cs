﻿using System;
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
                model.Problems = contest.Problems().OrderBy(n => n).ToList();
                model.PageCount = (int)Math.Ceiling(contest.AttendedUsersCount() / (double)STANDING_PAGE_SIZE);
                model.Contest = contest;
                model.StartIndex = model.PageIndex * STANDING_PAGE_SIZE;

                switch (contest.Type)
                {
                    case Contest.ContestType.ACM:
                        model.ACM = contest.GetACMStanding(model.StartIndex, STANDING_PAGE_SIZE, model.ShowVirtual);
                        break;
                    case Contest.ContestType.OI:
                        model.OI = contest.GetOIStanding(model.StartIndex, STANDING_PAGE_SIZE, model.ShowVirtual);
                        break;
                    case Contest.ContestType.CF:
                        model.CF = contest.GetCFStanding(model.StartIndex, STANDING_PAGE_SIZE, model.ShowVirtual);
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
                Minute = 0
            };

            return View(model);
        }

        [HttpPost]
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

            return RedirectToAction("Show", new { id = model.Name });
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
                model.Owner1 = USER.CurrentUserName;
                var otherOwners = con.Owners.Where(u => u != USER.CurrentUserName);
                model.Owner2 = otherOwners.FirstOrDefault();
                model.Owner3 = otherOwners.Skip(1).FirstOrDefault();
                model.StartTime = con.AbsoluteStartTime;
                model.Type = con.Type;
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "找不到相应比赛" });
            }
            return View(model);
        }

        [HttpPost]
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
                con.Change();
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

            return new EmptyResult();
        }

        public ActionResult Mine()
        {
            return View();
        }

        public ActionResult Manage()
        {
            return View();
        }

        public ActionResult Preview()
        {
            return View();
        }

        public ActionResult Complete()
        {
            return View();
        }
    }
}
