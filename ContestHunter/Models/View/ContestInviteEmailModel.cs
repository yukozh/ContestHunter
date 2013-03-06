using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace ContestHunter.Models.View
{
    public class ContestInviteEmailModel
    {
        public string Contest { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull=false)]
        public string Content { get; set; }
    }
}