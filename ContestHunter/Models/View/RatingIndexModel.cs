using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContestHunter.Models.Domain;
namespace ContestHunter.Models.View
{
    public class RatingIndexModel
    {
        public int PageIndex { get; set; }
        public int PageCount { get; set; }
        public int StartIndex { get; set; }
        public List<User> Users { get; set; }
    }
}