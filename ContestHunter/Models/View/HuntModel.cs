using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ContestHunter.Models.Domain;
namespace ContestHunter.Models.View
{
    public class HuntModel
    {
        public string HisName { get; set; }
        public string Problem { get; set; }
        public string Contest { get; set; }
        public string HisCode { get; set; }
        public Record.LanguageType HisLanguage { get; set; }

        public Record.LanguageType? MyLanague { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull=false)]
        public string MyCode { get; set; }
    }
}