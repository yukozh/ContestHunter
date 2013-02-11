using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContestHunter.Models.Domain
{
    public class Record
    {
        public string User;
        public string Contest;
        public string Problem;
        public string Code;
        public string Detail;
        
        public enum LanguageType
        {
            C,
            CPP,
            Pascal,
        };
        public LanguageType Language;
        public DateTime SubmitTime;

        public TimeSpan ExecutedTime;

        long Memory;
        int CodeLength;

        public enum StatusType
        {
            Accept,
            Wrong_Answer,
            Time_Limit_Execeeded,
            Runtime_Error,
            Memory_Limit_Execeeded,
            CMP_Error,
            Output_Limit_Execeeded,
            System_Error=-1,
            Pending=-2
        }

        public StatusType Status;

    }
}