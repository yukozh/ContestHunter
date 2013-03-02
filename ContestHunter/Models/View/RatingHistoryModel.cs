using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContestHunter.Models.Domain;
namespace ContestHunter.Models.View
{
    public class RatingHistoryModel
    {
        public List<Rating> Ratings { get; set; }
        public string User { get; set; }
    }
}