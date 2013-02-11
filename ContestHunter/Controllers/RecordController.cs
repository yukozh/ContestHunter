using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using ContestHunter.Models.Domain;
using ContestHunter.Models.View;
namespace ContestHunter.Controllers
{
    public class RecordController : Controller
    {
        static readonly int INDEX_PAGE_SIZE = int.Parse(ConfigurationManager.AppSettings["Record.Index.PageSize"]);

        public ActionResult Index()
        {
            ViewBag.Message = Record.Select(0, 10, null, null, null, null, null, null);
            return View();
        }

    }
}
