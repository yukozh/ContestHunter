using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ContestHunter.Models.Domain;
namespace ContestHunter.Models.View
{
    public class ProblemSubmitModel
    {
        [Required]
        public string Code { get; set; }
        [Required]
        public Record.LanguageType? Language { get; set; }

        public string Problem { get; set; }
        public string Contest { get; set; }
    }
}