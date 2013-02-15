using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ContestHunter.Models.Domain;
namespace ContestHunter.Models.View
{
    public class ContestStandingModel
    {
        public Contest Contest { get; set; }
        public List<string> Problems { get; set; }

        [Display(Name = "显示模拟比赛用户")]
        public bool ShowVirtual { get; set; }

        public int PageCount { get; set; }
        public int PageIndex { get; set; }
        public int StartIndex { get; set; }

        public List<ACMStanding> ACM { get; set; }
        public List<CFStanding> CF { get; set; }
        public List<OIStanding> OI { get; set; }

        public ContestStandingModel()
        {
            ShowVirtual = true;
        }
    }
}