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
            Delete, Change, Upload
        }

        public class TestCaseInfo
        {
            public string InputHash { get; set; }
            public string OutputHash { get; set; }

            [Required]
            [Display(Description = "MB")]
            [Range(0.0, double.MaxValue)]
            public double? Memory { get; set; }
            
            [Required]
            [Display(Description = "s")]
            [Range(0.0, double.MaxValue)]
            public double? Time { get; set; }
        };

        [Required]
        public ActionType? Action { get; set; }

        public Guid? TestCaseID { get; set; }
        public string Problem { get; set; }
        public string Contest { get; set; }
        public List<TestCaseInfo> TestCases { get; set; }

        public HttpPostedFileBase File { get; set; }
    }
}