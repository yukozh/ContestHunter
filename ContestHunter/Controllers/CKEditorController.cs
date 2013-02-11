using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
namespace ContestHunter.Controllers
{
    public class CKEditorController : Controller
    {
        static readonly int MAX_INLINE_FILE_SIZE = int.Parse(ConfigurationManager.AppSettings["MaxInlineFileSize"]);

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Upload(string CKEditorFuncNum, HttpPostedFileBase upload)
        {
            if (upload.ContentLength > MAX_INLINE_FILE_SIZE) return new HttpStatusCodeResult(400);

            ViewBag.FuncName = CKEditorFuncNum;
            byte[] bytes;
            using (MemoryStream mem = new MemoryStream())
            {
                upload.InputStream.CopyTo(mem);
                bytes = mem.ToArray();
            }
            ViewBag.URI = string.Format("data:{0};base64,{1}", upload.ContentType, Convert.ToBase64String(bytes));
            return View();
        }

    }
}
