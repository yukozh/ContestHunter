using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContestHunter.Models.Domain;
namespace ContestHunter.Models.View
{
    public class HuntListModel
    {
        public string UserName { get; set; }
        public string ContestName { get; set; }
        public string ProblemName { get; set; }
        public Hunt.StatusType? Status { get; set; }

        public List<Hunt> Hunts { get; set; }
        public int PageIndex { get; set; }
        public int PageCount { get; set; }
    }
}