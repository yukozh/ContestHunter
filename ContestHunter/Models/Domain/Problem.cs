using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContestHunter.Database;

namespace ContestHunter.Models.Domain
{
    public class Problem
    {
        public string Name;
        public string Content;
        public string Comparer;

        public override bool Equals(object obj)
        {
            if (obj is Problem)
                throw new Exception("Undefined Compare");
            return base.Equals(obj);
        }
    }
}