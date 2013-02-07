using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ContestHunter.Models.View.User;
using ContestHunter.Models.Domain;
using USER = ContestHunter.Models.Domain.User;

namespace ContestHunter.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            return View("Validate", model);
        }

        public ActionResult Validate()
        {
            return View();
        }
        public ActionResult Registered()
        {
            return View();
        }
    }
}
