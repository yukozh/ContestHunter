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
            model.Records = Record.Select(model.PageIndex * INDEX_PAGE_SIZE, INDEX_PAGE_SIZE, model.UserName, model.ProblemName, model.ContestName, model.Language, model.Status, model.OrderBy);
            model.PageCount = (int)Math.Ceiling(Record.Count() / (double)INDEX_PAGE_SIZE);
            return View(model);
        }

        public ActionResult Show(Guid id)
        {
            Record record;
            try
            {
                record = Record.ByID(id);
            }
            catch (ContestNotEndedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "比赛尚未结束，信息不予显示" });
            }
            catch (RecordNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有这个记录" });
            }
            return View(record);
        }
    }
}
