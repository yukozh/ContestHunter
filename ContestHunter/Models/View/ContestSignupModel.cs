using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ContestHunter.Models.Domain;
namespace ContestHunter.Models.View
{
    public class ContestSignupModel
    {
        public Contest Contest { get; set; }

        public enum SignupType
        {
            Attend, Virtual, Practice
        }

        [Required]
        public SignupType? Type { get; set; }

        [Display(Name = "开始时间", Description = "您希望模拟比赛何时开始")]
        public DateTime? StartTime { get; set; }
    }
}