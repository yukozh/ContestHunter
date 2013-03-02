using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using ContestHunter.Models.Domain;
using ContestHunter.Models.View;
using USER = ContestHunter.Models.Domain.User;
namespace ContestHunter.Controllers
{
    public class RatingController : Controller
    {
        static readonly int INDEX_PAGE_SIZE = int.Parse(ConfigurationManager.AppSettings["Rating.Index.PageSize"]);
        static readonly double GRAPH_WIDTH = 3000 * 1.618;

        [AllowAnonymous]
        public ActionResult Index(RatingIndexModel model)
        {
            if (model == null) model = new RatingIndexModel();
            model.Users = USER.List(model.PageIndex * INDEX_PAGE_SIZE, INDEX_PAGE_SIZE);
            model.PageCount = (int)Math.Ceiling(USER.Count() / (double)INDEX_PAGE_SIZE);
            model.StartIndex = model.PageIndex * INDEX_PAGE_SIZE;
            return View(model);
        }

        [HttpGet]
        public ActionResult History(string id)
        {
            RatingHistoryModel model = new RatingHistoryModel
            {
                User = id
            };
            try
            {
                USER user = USER.ByName(id);
                model.Ratings = user.RatingHistory();
            }
            catch (UserNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有这个用户" });
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult Graph(string id)
        {
            Response.ContentType = "image/svg+xml";
            RatingGraphModel model = new RatingGraphModel()
            {
                Width = GRAPH_WIDTH,
                Points = new List<RatingGraphModel.Point>()
            };
            var history = USER.ByName(id).RatingHistory();
            model.Times = history.Select(h => h.Time).ToList();
            if (history.Count == 0)
            {
            }
            else if (history.Count == 1)
            {
                model.Points.Add(new RatingGraphModel.Point { X = GRAPH_WIDTH / 2, Y = 3000 - history[0].Score });
            }
            else
            {
                double each = GRAPH_WIDTH / (history.Count - 1);
                for (int i = 0; i < history.Count; i++)
                {
                    model.Points.Add(new RatingGraphModel.Point { X = each * i, Y = 3000 - history[i].Score });
                }
            }
            return View(model);
        }
    }
}
