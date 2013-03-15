using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Concurrent;
using System.Threading;

namespace ContestHunter.Models.Domain
{
    public class AccessRestriction
    {
        class Accession
        {
            public int Pressure;
            public DateTime LastAccessTime;
        }
        static ConcurrentDictionary<string, Accession> accessTable = new ConcurrentDictionary<string, Accession>();
        public static void CheckRestriction(string ip)
        {
            string key = ip;
            if (null != User.CurrentUser)
                key = User.CurrentUserName;
            Accession ac = accessTable.GetOrAdd(key, new Accession()
            {
                Pressure = 0,
                LastAccessTime = DateTime.Now
            });
            lock (ac)
            {
                var span = DateTime.Now - ac.LastAccessTime;
                if (span.TotalSeconds <= 3)
                    ac.Pressure++;
                else if (span.TotalSeconds >= 30)
                    ac.Pressure -= (int)span.TotalSeconds / 30;
                Math.Max(0, ac.Pressure);
                ac.LastAccessTime = DateTime.Now;
            }
            if (ac.Pressure > 10)
                throw new AccessTooFrequentlyException();
        }
    }
}