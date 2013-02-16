using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using ContestHunter.Models.Domain;
using ContestHunter.Models.View;
using USER = ContestHunter.Models.Domain.User;

namespace ContestHunter.Controllers
{
    public class ProblemController : Controller
    {
        //
        // GET: /Problem/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Show(string id, string contest)
        {
            Problem problem;
            try
            {
                Contest con = Contest.ByName(contest);
                problem = con.ProblemByName(id);
                if (problem.Content == null)
                {
                    problem.Content = "<span style='color:red;'>无法查看题目内容</span>";
                }
                try
                {
                    var testCases = from tid in problem.TestCases()
                                    select problem.TestCaseByID(tid);

                    ViewBag.TotalTimeLimit = testCases.Select(t => t.TimeLimit).DefaultIfEmpty().Sum();
                    ViewBag.MinimumMemoryLimit = testCases.Select(t => t.MemoryLimit).DefaultIfEmpty().Min();
                    ViewBag.MaximumMemoryLimit = testCases.Select(t => t.MemoryLimit).DefaultIfEmpty().Max();
                }
                catch
                {
                    ViewBag.TotalTimeLimit = -1;
                    ViewBag.MinimumMemoryLimit = -1;
                    ViewBag.MaximumMemoryLimit = -1;
                }
                ViewBag.CanEdit = con.Owner.Contains(USER.CurrentUserName);
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "比赛不存在" });
            }
            catch (ProblemNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "题目不存在" });
            }
            return View(problem);
        }

        [HttpGet]
        public ActionResult Submit(string id, string contest)
        {
            ProblemSubmitModel model = new ProblemSubmitModel();
            model.Problem = id;
            model.Contest = contest;

            Problem problem;
            try
            {
                problem = Contest.ByName(model.Contest).ProblemByName(model.Problem);
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "比赛不存在" });
            }
            catch (ProblemNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "题目不存在" });
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Submit(ProblemSubmitModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            Guid recordID;
            try
            {
                Problem problem = Contest.ByName(model.Contest).ProblemByName(model.Problem);
                recordID = problem.Submit(new Record
                {
                    Code = model.Code,
                    Language = (Record.LanguageType)model.Language
                });
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "比赛不存在" });
            }
            catch (ContestNotStartedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "比赛尚未开始，不能提交代码" });
            }
            catch (NotAttendedContestException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有报名比赛，不能提交代码" });
            }
            catch (UserNotLoginException)
            {
                throw;
            }
            catch (ProblemNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "题目不存在" });
            }
            catch (ProblemLockedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "题目被锁定，无法提交代码" });
            }

            return RedirectToAction("Show", "Record", new { id = recordID });
        }

        [HttpGet]
        public ActionResult Add(string contest)
        {
            ProblemBasicInfoModel model = new ProblemBasicInfoModel
            {
                Contest = contest
            };
            try
            {
                Contest con = Contest.ByName(contest);
                model.ContestType = con.Type;
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有这场比赛" });
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Add(ProblemBasicInfoModel model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                Contest contest = Contest.ByName(model.Contest);
                if (contest.Type == Contest.ContestType.CF && model.OriginalRating == null)
                {
                    ModelState.AddModelError("OriginalRating", "不可为空");
                    return View(model);
                }
                contest.AddProblem(new Problem
                {
                    Content = System.IO.File.ReadAllText(Server.MapPath("~/Content/ProblemTemplate.html")),
                    Name = model.Name,
                    Comparer = "",
                    DataChecker = "",
                    OriginRating = model.OriginalRating,
                    Owner = model.Owner
                });
            }
            catch (ProblemNameExistedException)
            {
                ModelState.AddModelError("Name", "题目名已存在");
                return View(model);
            }
            catch (UserNotFoundException)
            {
                ModelState.AddModelError("Owner", "没有这个用户");
                return View(model);
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没找到相应比赛" });
            }
            catch (PermissionDeniedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有添加比赛权限" });
            }
            catch (UserNotLoginException)
            {
                throw;
            }
            return RedirectToAction("Description", new { id = model.Name, contest = model.Contest });
        }

        [HttpGet]
        public ActionResult Edit(string id, string contest)
        {
            ProblemBasicInfoModel model = new ProblemBasicInfoModel
            {
                Contest = contest,
                OldName = id
            };
            try
            {
                Contest con = Contest.ByName(contest);
                Problem problem = con.ProblemByName(id);
                model.Owner = problem.Owner;
                model.OriginalRating = problem.OriginRating;
                model.ContestType = con.Type;
                model.Name = problem.Name;
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有找到相应的比赛" });
            }
            catch (ProblemNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有这个题目" });
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(ProblemBasicInfoModel model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                Contest contest = Contest.ByName(model.Contest);
                if (contest.Type == Contest.ContestType.CF && model.OriginalRating == null)
                {
                    ModelState.AddModelError("OriginalRating", "不可为空");
                    return View(model);
                }
                contest.AddProblem(new Problem
                {
                    Content = System.IO.File.ReadAllText(Server.MapPath("~/Content/ProblemTemplate.html")),
                    Name = model.Name,
                    Comparer = "",
                    DataChecker = "",
                    OriginRating = model.OriginalRating,
                    Owner = model.Owner
                });
            }
            catch (ProblemNameExistedException)
            {
                ModelState.AddModelError("Name", "题目名已存在");
                return View(model);
            }
            catch (UserNotFoundException)
            {
                ModelState.AddModelError("Owner", "没有这个用户");
                return View(model);
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没找到相应比赛" });
            }
            catch (PermissionDeniedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有添加比赛权限" });
            }
            catch (UserNotLoginException)
            {
                throw;
            }
            return RedirectToAction("Description", new { id = model.Name, contest = model.Contest });
        }

        [HttpGet]
        public ActionResult Description(string contest, string id)
        {
            ProblemContentModel model = new ProblemContentModel
            {
                Contest = contest,
                Problem = id
            };
            try
            {
                Problem problem = Contest.ByName(contest).ProblemByName(id);
                if (problem.Content == null)
                {
                    return RedirectToAction("Error", "Shared", new { msg = "无法查看题目内容" });
                }
                model.Content = problem.Content;
                model.ProblemOwner = problem.Owner;
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有找到所需的比赛" });
            }
            catch (ProblemNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有找到所需的题目" });
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Description(ProblemContentModel model)
        {
            if (!ModelState.IsValid) return View(model);
            switch (model.Action)
            {
                case ProblemContentModel.ActionType.Preview:
                    return View(model);
                case ProblemContentModel.ActionType.Modify:
                    try
                    {
                        Problem problem = Contest.ByName(model.Contest).ProblemByName(model.Problem);
                        problem.Content = model.Content;
                        problem.Change();
                    }
                    catch (ContestNotFoundException)
                    {
                        return RedirectToAction("Error", "Shared", new { msg = "没有找到相应比赛" });
                    }
                    catch (ProblemNotFoundException)
                    {
                        return RedirectToAction("Error", "Shared", new { msg = "没有找到相应题目" });
                    }
                    return new EmptyResult();
            }
            throw new NotImplementedException();
        }

        public ActionResult Config()
        {
            return View();
        }

        public ActionResult Check()
        {
            return View();
        }

        public ActionResult Complete()
        {
            return View();
        }
    }
}
