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
            public int InputSize { get; set; }
            public int OutputSize { get; set; }
            public string Input { get; set; }
            public string Output { get; set; }

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

        [Display(Name="测试数据压缩包",Description=@"请将所有数据，压缩为一个.zip文件。<br/>所有输入文件名应匹配 <b>(\d+)\.in(put)?$</b> ")]
        public HttpPostedFileBase File { get; set; }
    }
}