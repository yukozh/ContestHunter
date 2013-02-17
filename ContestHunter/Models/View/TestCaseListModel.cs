using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace ContestHunter.Models.View
{
    public class TestCaseListModel
    {
        public string Problem { get; set; }
        public string Contest { get; set; }
        public List<TestCaseInfo> TestCases { get; set; }
    }
}