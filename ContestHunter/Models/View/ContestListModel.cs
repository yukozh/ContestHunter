using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContestHunter.Models.Domain;
namespace ContestHunter.Models.View
{
    public class ContestListModel
    {
        public int PendingPageIndex { get; set; }
        public int TestingPageIndex { get; set; }
        public int DonePageIndex { get; set; }

        public int PendingPageCount { get; set; }
        public int TestingPageCount { get; set; }
        public int DonePageCount { get; set; }

        public List<Contest> Pending { get; set; }
        public List<Contest> Testing { get; set; }
        public List<Contest> Done { get; set; }
    }
}