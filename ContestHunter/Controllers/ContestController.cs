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
        public ActionResult Index(int pagePending = 0, int pageTesting = 0, int pageDone = 0)
        {
            ViewBag.Pending = Contest.Pending(pagePending * INDEX_PAGE_SIZE, INDEX_PAGE_SIZE);
            ViewBag.Testing = Contest.Testing(pageTesting * INDEX_PAGE_SIZE, INDEX_PAGE_SIZE);
            ViewBag.Done = Contest.Done(pageDone * INDEX_PAGE_SIZE, INDEX_PAGE_SIZE);
            
            return View();
        }
        public ActionResult List()
        {
            return View();
        }
    }
}
