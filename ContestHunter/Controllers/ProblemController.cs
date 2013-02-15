using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
                problem = Contest.ByName(contest).ProblemByName(id);
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "比赛不存在" });
            }
            catch (ContestNotStartedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "比赛尚未开始，题目不予显示" });
            }
            catch (NotAttendedContestException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有报名比赛，题目不予显示" });
            }
            catch (UserNotLoginException)
            {
                throw;
            }
            catch (ProblemNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "题目不存在" });
            }
            var testCases = from tid in problem.TestCases()
                            select problem.TestCaseByID(tid);

            ViewBag.TotalTimeLimit = testCases.Select(t => t.TimeLimit).DefaultIfEmpty().Sum();
            ViewBag.MinimumMemoryLimit = testCases.Select(t => t.MemoryLimit).DefaultIfEmpty().Min();
            ViewBag.MaximumMemoryLimit = testCases.Select(t => t.MemoryLimit).DefaultIfEmpty().Max();
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
                recordID=problem.Submit(new Record
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

            return RedirectToAction("Show", "Record", new { id = recordID });
        }

        public ActionResult Add()
        {
            return View();
        }

        public ActionResult Edit()
        {
            return View();
        }

        public ActionResult Description(string html = "")
        {
            ViewBag.Html = html;
            return View();
        }

        public ActionResult Config()
        {
            return View();
        }

        public ActionResult Test()
        {
            return View();
        }
    }
}
