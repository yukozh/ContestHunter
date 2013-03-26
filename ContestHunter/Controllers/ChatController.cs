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

        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous] //便于调试
        public ActionResult Public()
        {
            return View();
        }
    }
}
