using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using ContestHunter.Models.Domain;

namespace ContestHunter.Controllers
{
    public class ContestController : Controller
    {
        static readonly int INDEX_PAGE_SIZE = int.Parse(ConfigurationManager.AppSettings["Contest.Index.PageSize"]);

        [AllowAnonymous]
        public ActionResult Index(int pendingPageIndex = 0, int testingPageIndex = 0, int donePageIndex = 0)
        {
            ViewBag.Pending = Contest.Pending(pendingPageIndex * INDEX_PAGE_SIZE, INDEX_PAGE_SIZE);
            ViewBag.Testing = Contest.Testing(testingPageIndex * INDEX_PAGE_SIZE, INDEX_PAGE_SIZE);
            ViewBag.Done = Contest.Done(donePageIndex * INDEX_PAGE_SIZE, INDEX_PAGE_SIZE);

            ViewBag.PendingPageCount = (int)Math.Ceiling(Contest.PendingCount() / (double)INDEX_PAGE_SIZE);
            ViewBag.TestingPageCount = (int)Math.Ceiling(Contest.TestingCount() / (double)INDEX_PAGE_SIZE);
            ViewBag.DonePageCount = (int)Math.Ceiling(Contest.DoneCount() / (double)INDEX_PAGE_SIZE);

            ViewBag.PendingPageIndex = pendingPageIndex;
            ViewBag.TestingPageIndex = testingPageIndex;
            ViewBag.DonePageIndex = donePageIndex;

            return View();
        }
        public ActionResult List()
        {
            return View();
        }
    }
}
