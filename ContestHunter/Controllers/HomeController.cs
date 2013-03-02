using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ContestHunter.Models.View;
using ContestHunter.Models.Domain;
using USER = ContestHunter.Models.Domain.User;
namespace ContestHunter.Controllers
{
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            HomeIndexModel model = new HomeIndexModel
            {
                Rating = USER.List(0, 10)
            };
            return View(model);
        }
    }
}
