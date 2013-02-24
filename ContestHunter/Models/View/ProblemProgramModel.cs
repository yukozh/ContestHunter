using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ContestHunter.Models.Domain;
namespace ContestHunter.Models.View
{
    public class ProblemProgramModel
    {
        public Record.LanguageType StdLanguage { get; set; }
        public Record.LanguageType SpjLanguage { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull=false)]
        public string Std { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull=false)]
        public string Spj { get; set; }

        public string Contest { get; set; }
        public string Problem { get; set; }
    }
}