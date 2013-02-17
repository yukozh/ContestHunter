using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContestHunter.Models.Domain
{
    public class ACMStanding
    {
        public string User;
        public int TotalTime;
        public int CountAC;
        public bool IsVirtual;
        public class DescriptionClass
        {
            public bool isAC;
            public int FailedTimes;
            public int? ACTime;
        }
        public List<DescriptionClass> Description;
    }

    public class CFStanding
    {
        public string User;
        public class DescriptionClass
        {
            public bool isAC;
            public int FailedTimes;
            public int? ACTime;
            public int Rating;
            internal int _huntFailed;
            internal int _huntSuccessfully;
        }
        public List<DescriptionClass> Description;
        public int TotalRating;
        public int SuccessHack;
        public int FailedHack;
        public bool IsVirtual;
    }

    public class OIStanding
    {
        public string User;
        public List<int?> Scores;
        public int TotalScore;
        public int TotalTime;
        public bool IsVirtual;
    }

}