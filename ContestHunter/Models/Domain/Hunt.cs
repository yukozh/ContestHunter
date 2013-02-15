using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContestHunter.Models.Domain
{
    public class Hunt
    {
        public enum Status
        {
            Pending,
            Success,
            Fail
        }
    }
}