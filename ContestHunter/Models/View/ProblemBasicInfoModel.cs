using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ContestHunter.Models.Domain;
namespace ContestHunter.Models.View
{
    public class ProblemBasicInfoModel
    {
        public string Contest { get; set; }
        public string Problem { get; set; }
        public Contest.ContestType ContestType { get; set; }

        [Required]
        [Display(Name = "题目名称")]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "出题人", Description = "出题人的用户名")]
        [MaxLength(50)]
        public string Owner { get; set; }

        [Display(Name = "初始分数", Description = "此题的初始最高分值，随着比赛的进行，实际分值会不断减少")]
        [Range(0, 3000)]
        public int? OriginalRating { get; set; }
    }
}