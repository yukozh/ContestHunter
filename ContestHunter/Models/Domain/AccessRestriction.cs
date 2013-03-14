using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContestHunter.Models.Domain
{
    public class AccessRestriction
    {
        static Dictionary<string, Queue<DateTime>> accessTable = new Dictionary<string, Queue<DateTime>>();

        public static void CheckRestriction(string ip)
        {
            string key=ip;
            if (null != User.CurrentUser)
                key = User.CurrentUserName;
            lock (accessTable)
            {
                if (!accessTable.ContainsKey(key))
                    accessTable.Add(key, new Queue<DateTime>());
                var queue = accessTable[key];
                if (queue.Count > 0 && DateTime.Now - queue.Last() < TimeSpan.FromSeconds(1))
                    throw new AccessTooFrequentlyException();
                var poptime = DateTime.Now - TimeSpan.FromMinutes(5);
                while (queue.Count > 0 && queue.First() < poptime)
                    queue.Dequeue();
                if (queue.Count > 100)
                    throw new AccessTooFrequentlyException();
                queue.Enqueue(DateTime.Now);
            }
        }
    }
}