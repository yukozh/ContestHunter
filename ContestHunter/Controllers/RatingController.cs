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
        static readonly int INDEX_PAGE_SIZE=int.Parse(ConfigurationManager.AppSettings["Rating.Index.PageSize"]);

        [AllowAnonymous]
        public ActionResult Index(RatingIndexModel model)
        {
            if (model == null) model = new RatingIndexModel();
            model.Users=USER.List(model.PageIndex * INDEX_PAGE_SIZE, INDEX_PAGE_SIZE);
            model.PageCount = (int)Math.Ceiling(USER.Count() / (double)INDEX_PAGE_SIZE);
            model.StartIndex = model.PageIndex * INDEX_PAGE_SIZE;
            return View(model);
        }

        public ActionResult History()
        {
            return View();
        }
    }
}
