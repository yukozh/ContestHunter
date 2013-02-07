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
            if (USER.IsNameExisted(model.Name))
            {
                ModelState.AddModelError("Name", "用户名已存在");
                return View(model);
            }

            try
            {
                USER.SendValidationEmail(model.Name, model.Password, model.Email, Url.Action("ValidateEmail", "User", null, "http"));
            }
            catch
            {
                ModelState.AddModelError("Email", "无法发送到此邮箱");
                return View(model);
            }
            return View("CheckEmail", model);
        }

        public ActionResult CheckEmail()
        {
            return View();
        }
        public ActionResult Registered()
        {
            return View();
        }
    }
}
