using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContestHunter.Models.Domain;
namespace ContestHunter.Models.View
{
    public class ContestStandingModel
    {
        public string Contest { get; set; }
        public List<string> Problems { get; set; }
        public Contest.ContestType Type { get; set; }

        public int PageCount { get; set; }
        public int PageIndex { get; set; }
        public int StartIndex { get; set; }

        public List<ACMStanding> ACM { get; set; }
        public List<CFStanding> CF { get; set; }
    }
}