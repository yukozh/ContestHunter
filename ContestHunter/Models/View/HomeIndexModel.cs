using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ContestHunter.Models.Domain;

namespace ContestHunter.Models.View
{
    public class HomeIndexModel
    {
        public List<User> Rating { get; set; }
    }
}