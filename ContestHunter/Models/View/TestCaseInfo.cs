using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace ContestHunter.Models.View
{
    public class TestCaseInfo
    {
        public Guid ID { get; set; }
        public int InputSize { get; set; }
        public int OutputSize { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }

        [Required]
        [Display(Description = "MB")]
        [Range(0.0, double.MaxValue)]
        public double? Memory { get; set; }

        [Required]
        [Display(Description = "s")]
        [Range(0.0, double.MaxValue)]
        public double? Time { get; set; }

        [Required]
        public bool Enabled { get; set; }
    }
}