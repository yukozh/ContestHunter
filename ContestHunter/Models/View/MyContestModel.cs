using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContestHunter.Models.View
{
    public class MyContestModel
    {
        public class ContestInfo
        {
            public string Name { get; set; }
            public List<string> OtherOwners { get; set; }
            public int AttendUserCount { get; set; }
            public DateTime StartTime { get; set; }
        }

        public List<ContestInfo> Contests { get; set; }
    }
}