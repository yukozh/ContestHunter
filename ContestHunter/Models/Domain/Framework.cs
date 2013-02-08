using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContestHunter.Database;

namespace ContestHunter.Models.Domain
{
    public class Framework
    {
        static void DomainInstallation()
        {
            using (var db = new CHDB())
            {
                if (!db.Database.Exists())
                {
                    throw new Exception("Database not found");
                }
                
            }
        }
    }
}