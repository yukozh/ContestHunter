using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace ContestHunter.Models.View
{
    public class ProblemBasicInfoModel
    {
        public string Contest { get; set; }

        [Required]
        [Display(Name="题目名称")]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "出题人", Description = "出题人的用户名")]
        [MaxLength(50)]
        public string Owner { get; set; }
    }
}