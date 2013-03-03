using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using ContestHunter.Models.View;
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
        [ValidateAntiForgeryToken]
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
                string returnUrl=Request.Url.GetLeftPart(UriPartial.Authority) + Url.Action("ValidateEmail", "User");
                USER.SendValidationEmail(model.Name, model.Password, model.Email, returnUrl);
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
        [ValidateAntiForgeryToken]
        public ActionResult CheckPassword(RegisterModel model, string passwordHash, string emailHash)
        {
            ViewBag.EmailHash = emailHash;
            ViewBag.PasswordHash = passwordHash;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                USER.Register(model.Name, model.Password, passwordHash, model.Email, emailHash, "中国", "山东省", "青岛市", "第二中学", "我是神犇", "王强松");
            }
            catch (PasswordMismatchException)
            {
                ModelState.AddModelError("Password", "输入的密码与注册时不匹配");
                return View(model);
            }
            catch (EmailMismatchException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "验证邮件非法" });
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
            LoginModel model = new LoginModel
            {
                KeepOnline = true
            };
            return View(model);
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
                token = USER.Login(model.UserName, model.Password, Request.Headers["X-Forwarded-For"] ?? Request.UserHostAddress);
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

            try
            {
                user = USER.ByName(id);
            }
            catch (UserNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "找不到指定的用户" });
            }

            return View(user);
        }

        [HttpGet]
        public ActionResult Edit()
        {
            USER user = USER.ByName(USER.CurrentUserName);
            UserEditModel model = new UserEditModel
            {
                City=user.City,
                Country=user.Country,
                Email=user.Email,
                Motto=user.Motto,
                Province=user.Province,
                RealName=user.RealName,
                School=user.School
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserEditModel model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                USER user = USER.ByName(USER.CurrentUserName);
                user.City = model.City;
                user.Country = model.Country;
                user.Motto = model.Motto;
                if (model.NewPassword != null)
                    user.Password = model.NewPassword;
                user.Province = model.Province;
                user.RealName = model.RealName;
                user.School = model.School;
                user.Change(model.OldPassword);
            }
            catch (UserNotFoundException)
            {
                throw;
            }
            catch (UserNotLoginException)
            {
                throw;
            }
            catch (PermissionDeniedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "由于某种原因，您无权更改用户信息" });
            }
            catch (PasswordMismatchException)
            {
                ModelState.AddModelError("OldPassword", "密码不正确");
                return View(model);
            }

            return RedirectToAction("Show", new { id = USER.CurrentUserName });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetAdmin(string id) {
            try
            {
                Group.ByName("Administrators").AddUser(USER.ByName(id));
            }
            catch (GroupNotFoundException)
            {
                throw;
            }
            catch (PermissionDeniedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "您无权设置管理员" });
            }
            return RedirectToAction("Show", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UnsetAdmin(string id)
        {
            try
            {
                Group.ByName("Administrators").RemoveUser(USER.ByName(id));
            }
            catch (GroupNotFoundException)
            {
                throw;
            }
            catch (PermissionDeniedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "您无权取消管理员" });
            }
            return RedirectToAction("Show", new { id = id });
        }
    }
}
