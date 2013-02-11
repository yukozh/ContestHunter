using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContestHunter.Models.View
{
    public class RecordListModel
    {
        public string UserName { get; set; }
        public string ContestName { get; set; }
        public string Status { get; set; }
        public string Language { get; set; }
        public int PageIndex { get; set; }
    }
}