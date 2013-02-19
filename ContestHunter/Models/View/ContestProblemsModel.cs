using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ContestHunter.Models.Domain;

namespace ContestHunter.Models.View
{
    public class ContestProblemsModel
    {
        public string Contest { get; set; }
        public List<ProblemInfo> Problems { get; set; }
        public int ProblemIndex { get; set; }
    }
}