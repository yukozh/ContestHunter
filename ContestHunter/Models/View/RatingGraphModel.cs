using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContestHunter.Models.View
{
    public class RatingGraphModel
    {
        public struct Point
        {
            public double X, Y;
        }

        public List<Point> Points { get; set; }
        public List<DateTime> Times { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
    }
}