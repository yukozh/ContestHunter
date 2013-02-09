using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using ContestHunter.Models.View.User;
using ContestHunter.Models.Domain;
using USER = ContestHunter.Models.Domain.User;

namespace ContestHunter.Controllers
{
    public class UserController : Controller
    {
        #region Register
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            if (USER.IsNameExisted(model.Name))
            {
                ModelState.AddModelError("Name", "用户名已存在");
                return View(model);
            }

            if (USER.IsEmailExisted(model.Email))
            {
                ModelState.AddModelError("Email", "Email已被使用");
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

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ValidateEmail(string name, string password, string email, string emailHash)
        {
            RegisterModel model = new RegisterModel
            {
                Name = name,
                Email = email,
            };
            ViewBag.EmailHash = emailHash;
            ViewBag.PasswordHash = password;
            return View("CheckPassword", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult CheckPassword(RegisterModel model, string passwordHash, string emailHash)
        {
            ViewBag.EmailHash = emailHash;
            ViewBag.PasswordHash = passwordHash;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                USER.Register(model.Name, model.Password, passwordHash, model.Email, emailHash);
            }
            catch (PasswordMismatchException)
            {
                ModelState.AddModelError("Password", "输入的密码与注册时不匹配");
                return View(model);
            }
            catch (EmailMismatchException)
            {
                return RedirectToAction("Error", "Shared", "验证邮件非法");
            }

            return View("Registered", model);
        }
        #endregion

        #region Login & Logout
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            if (!ModelState.IsValid)
                return View(model);

            string token;
            try
            {
                token = USER.Login(model.UserName, model.Password);
            }
            catch (UserNotFoundException)
            {
                ModelState.AddModelError("UserName", "用户名不存在");
                return View(model);
            }
            catch (PasswordMismatchException)
            {
                ModelState.AddModelError("Password", "密码错误");
                return View(model);
            }

            FormsAuthentication.SetAuthCookie(token, model.KeepOnline);

            return Redirect(ReturnUrl ?? Url.Action("Index", "Home"));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            USER.Logout();
            FormsAuthentication.SignOut();
            return Redirect("~");
        }
        #endregion

        public ActionResult Show(string id)
        {
            User user;

            try{
                user=USER.SelectByName(id);
            }catch(UserNotFoundException){
                return RedirectToAction("Error", "Shared", "找不到指定的用户");
            }

            return View(user);
        }
    }
}
