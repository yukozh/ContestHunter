using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ContestHunter.Models.Domain;
namespace ContestHunter.Models.View
{
    public class TestCaseUploadModel
    {
        public enum ActionType
        {
            Delete, Upload, Next
        }

        [Required]
        public ActionType? Action { get; set; }

        public int TestCaseIndex { get; set; }
        public string Problem { get; set; }
        public string Contest { get; set; }
        public bool ShowEnabled { get; set; }
        public List<TestCaseInfo> TestCases { get; set; }

        [Display(Name = "测试数据压缩包")]
        public HttpPostedFileBase File { get; set; }
    }
}