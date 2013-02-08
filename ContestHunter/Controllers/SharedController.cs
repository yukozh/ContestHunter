using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ContestHunter.Controllers
{
    public class SharedController : Controller
    {
        [AllowAnonymous]
        public ActionResult Error(string msg)
        {
            return View("Error",(object)msg);
        }
    }
}
