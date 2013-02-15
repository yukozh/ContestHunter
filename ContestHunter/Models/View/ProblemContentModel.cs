using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ContestHunter.Models.View
{
    public class ProblemContentModel
    {
        public enum ActionType
        {
            Preview, Modify
        }

        [Required]
        public ActionType? Action { get; set; }

        public string Contest { get; set; }
        public string Problem { get; set; }

        [Display(Name="题目内容")]
        public string Content { get; set; }
    }
}