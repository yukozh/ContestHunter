using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using ContestHunter.Models.Domain;
using ContestHunter.Models.View;
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

        [AllowAnonymous]
        public ActionResult Show(string id)
        {
            Contest contest;
            try
            {
                contest = Contest.ByName(id);
            }
            catch
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有相应的比赛" });
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
                model.Contest = id;
                model.Type = contest.Type;
                model.StartIndex = model.PageIndex * STANDING_PAGE_SIZE;

                switch (contest.Type)
                {
                    case Contest.ContestType.ACM:
                        model.ACM = contest.GetACMStanding(model.StartIndex, STANDING_PAGE_SIZE);
                        break;
                    case Contest.ContestType.OI:
                        model.OI = contest.GetOIStanding(model.StartIndex, STANDING_PAGE_SIZE);
                        break;
                    default:
                        throw new NotImplementedException();
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
    }
}
