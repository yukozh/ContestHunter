using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace ContestHunter.Models.View
{
    public class TestCaseUploadModel
    {
        public enum ActionType
        {
            Delete,Change,Upload
        }

        [Required]
        public ActionType? Action { get; set; }

        public HttpPostedFileBase File { get; set; }
    }
}