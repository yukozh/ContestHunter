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

        [AllowAnonymous]
        public ActionResult Index(RecordListModel model)
        {
            List<Record> records;
            records = Record.Select(model.PageIndex * INDEX_PAGE_SIZE, INDEX_PAGE_SIZE, model.UserName, model.ProblemName, model.ContestName, model.Language, model.Status, Record.OrderByType.SubmitTime);
            model.Records = records;
            model.PageCount = 123;
            return View(model);
        }

    }
}
