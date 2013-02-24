using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace ContestHunter.Models.View
{
    public class ContestDescriptionModel
    {
        public enum ActionType
        {
            Preview, Next
        }

        [Required]
        public ActionType? Action { get; set; }

        public string Contest { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull=false)]
        public string Description { get; set; }
    }
}