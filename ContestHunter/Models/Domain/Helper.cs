using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContestHunter.Models.Domain
{
    internal class Helper
    {
        public static string GetLegalName(string name)
        {
            return name.Replace('+', '＋').Replace('.', '．').Replace('/', '／').Replace('#', '＃');
        }
    }
}