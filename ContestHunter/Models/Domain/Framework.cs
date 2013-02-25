using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContestHunter.Database;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace ContestHunter.Models.Domain
{
    public class Framework
    {
        public static void DomainInstallation()
        {
            using (var db = new CHDB())
            {
                db.GROUPs.Add(new GROUP()
                {
                    ID = Guid.NewGuid(),
                    Name = "Administrators"
                });
                using (var sha = SHA256.Create())
                {
                    db.USERs.Add(new USER()
                    {
                        ID = Guid.NewGuid(),
                        Name = "Administrator",
                        Email = "Admin@ContestHunter.com",
                        Password = sha.ComputeHash(Encoding.Unicode.GetBytes("07070078899"))
                    });
                }
                db.SaveChanges();
                (from u in db.USERs
                 where u.Name == "Administrator"
                 select u).Single().GROUPs.Add
                (
                (from g in db.GROUPs
                    where g.Name == "Administrators"
                    select g).Single()
                );
                db.SaveChanges();
            }
        }
        static ContestDaemon contest = new ContestDaemon();
        static AllKorrectDaemon tester = new AllKorrectDaemon();
        public static void DomainStart()
        {
            contest.Start();
            tester.Start();
        }

        public static void DomainStop()
        {
            contest.Stop();
            tester.Stop();
        }
    }
}