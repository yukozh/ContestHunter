using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.IO;
using System.Text;
using ContestHunter.Models.Domain;
using ContestHunter.Models.View;
using HUNT = ContestHunter.Models.Domain.Hunt;
namespace ContestHunter.Controllers
{
    public class RecordController : Controller
    {
        static readonly int INDEX_PAGE_SIZE = int.Parse(ConfigurationManager.AppSettings["Record.Index.PageSize"]);
        static readonly int HUNT_LIST_PAGE_SIZE = int.Parse(ConfigurationManager.AppSettings["Record.HuntList.PageSize"]);
        static readonly int CODE_IMAGE_MAX_WIDTH = int.Parse(ConfigurationManager.AppSettings["Record.CodeImage.MaxWidth"]);
        static readonly int CODE_IMAGE_MAX_HEIGHT = int.Parse(ConfigurationManager.AppSettings["Record.CodeImage.MaxHeight"]);

        [AllowAnonymous]
        public ActionResult Index(RecordListModel model)
        {
            model.Records = Record.Select(model.PageIndex * INDEX_PAGE_SIZE, INDEX_PAGE_SIZE, model.UserName, model.ProblemName, model.ContestName, model.Language, model.Status, model.OrderBy);
            model.PageCount = (int)Math.Ceiling(Record.Count(model.UserName, model.ProblemName, model.ContestName, model.Language, model.Status) / (double)INDEX_PAGE_SIZE);
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
            HuntModel model = new HuntModel()
            {
                MyLanague=Record.LanguageType.Data,
                Record=id
            };
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
        public ActionResult Hunt(Guid id, HuntModel model)
        {
            if (!ModelState.IsValid) return View(model);
            Guid huntID;
            try
            {
                Record record = Record.ByID(id);
                huntID = record.Hunt(model.MyCode, model.MyLanague);
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
            catch (ProblemNotPassedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "您的相应题目没有通过" });
            }
            return RedirectToAction("HuntResult", new { id = huntID });
        }

        [HttpGet]
        public ActionResult HuntResult(Guid id)
        {
            HUNT hunt = HUNT.ByID(id);
            return View(hunt);
        }

        public ActionResult HuntList(HuntListModel model)
        {
            if (model == null) model = new HuntListModel();
            model.Hunts = HUNT.List(HUNT_LIST_PAGE_SIZE, HUNT_LIST_PAGE_SIZE * model.PageIndex, model.UserName, model.ContestName, model.ProblemName, model.Status);
            model.PageCount = (int)Math.Ceiling(HUNT.Count(model.UserName, model.ContestName, model.ProblemName, model.Status) / (double)HUNT_LIST_PAGE_SIZE);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Rejudge(Guid id)
        {
            try
            {
                Record.ByID(id).ReJudge();
            }
            catch (RecordNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有那条记录" });
            }
            catch (PermissionDeniedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "您没有权限重测记录" });
            }
            return RedirectToAction("Show", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RejudgeHunt(Guid id)
        {
            try
            {
                HUNT.ByID(id).ReJudge();
            }
            catch (UserNotLoginException)
            {
                throw;
            }
            catch (PermissionDeniedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "您没有权限重测猎杀记录" });
            }
            return RedirectToAction("HuntResult", new { id = id });
        }

        [HttpGet]
        public ActionResult CodeImage(Guid id, int width)
        {
            width = Math.Max(0, Math.Min(width, CODE_IMAGE_MAX_WIDTH));
            try
            {
                Record record = Record.ByID(id);
                if (string.IsNullOrWhiteSpace(record.Code)) record.Code = "The code is empty~";
                var img = new Imager.SourceImager().Generate(record.Code, width, CODE_IMAGE_MAX_HEIGHT);
                using (MemoryStream mem = new MemoryStream())
                {
                    img.Save(mem, System.Drawing.Imaging.ImageFormat.Png);
                    return File(mem.ToArray(), "image/png");
                }
            }
            catch (ContestNotEndedException)
            {
                return RedirectToAction("Error", "Show", new { msg = "比赛尚未结束，不可查看代码" });
            }
            catch (RecordNotFoundException)
            {
                return RedirectToAction("Error", "Show", new { msg = "您要找的记录不存在" });
            }
            catch (ProblemNotLockedException)
            {
                return RedirectToAction("Error", "Show", new { msg = "您的相应题目尚未锁定" });
            }
            catch (ProblemNotPassedException)
            {
                return RedirectToAction("Error", "Show", new { msg = "您的相应题目没有通过" });
            }
            catch (UserNotLoginException)
            {
                throw;
            }
        }
    }
}
