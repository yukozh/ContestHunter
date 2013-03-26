using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ContestHunter.Controllers
{
    public class ChatController : Controller
    {
        //
        // GET: /Chat/
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Dbg()
        {
            return View();
        }

        public ActionResult Public()
        {
            return View();
        }
    }
}
