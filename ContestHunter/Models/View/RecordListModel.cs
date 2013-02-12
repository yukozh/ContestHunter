using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ContestHunter.Models.Domain;
namespace ContestHunter.Models.View
{
    public class RecordListModel
    {
        public string UserName { get; set; }
        public string ProblemName { get; set; }
        public string ContestName { get; set; }
        public Record.StatusType? Status { get; set; }
        public Record.LanguageType? Language { get; set; }
        public int PageIndex { get; set; }
        public int PageCount { get; set; }
        public List<Record> Records { get; set; }
    }
}