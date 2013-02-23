using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Text;
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
            catch (ProblemNotLockedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "您尚未锁定题目，信息不予显示" });
            }
            catch (RecordNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有这个记录" });
            }
            return View(record);
        }

        [HttpGet]
        public ActionResult Hunt(Guid id)
        {
            HuntModel model = new HuntModel();
            try
            {
                Record record = Record.ByID(id);
                model.HisCode = record.Code;
                model.HisLanguage = record.Language;
                model.HisName = record.User;
                model.Problem = record.Problem;
                model.Contest = record.Contest;
            }
            catch (ContestNotEndedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "比赛尚未结束，不可查询代码" });
            }
            catch (RecordNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "您要猎杀的记录不存在" });
            }
            catch (ProblemNotLockedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "您的相应题目没有锁定" });
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Hunt(Guid id,HuntModel model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                Record record = Record.ByID(id);
                record.Hunt(Encoding.UTF8.GetBytes(model.MyCode));
            }
            catch (ContestNotEndedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "比赛尚未结束，不可查询代码" });
            }
            catch (ContestEndedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "比赛已结束，不可Hunt" });
            }
            catch (RecordNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "您要猎杀的记录不存在" });
            }
            catch (RecordStatusMismatchException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "此条记录测试结果不为“正确”" });
            }
            catch (ContestTypeMismatchException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "这场比赛不是Codeforces赛制" });
            }
            catch (ProblemNotLockedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "您的相应题目没有锁定" });
            }
            return RedirectToAction("Show", new { id = id });
        }
    }
}
