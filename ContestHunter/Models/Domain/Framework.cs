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
        public static string WebRoot;

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

        static void HuntBadWating(CHDB db)
        {
            foreach (var h in (from h in db.HUNTs
                               where h.Status == (int)Hunt.StatusType.Running
                               select h))
                h.Status = (int)Hunt.StatusType.Pending;
            db.SaveChanges();
        }

        static void RecordBadWating(CHDB db)
        {
            foreach (var h in (from h in db.RECORDs
                               where h.Status == (int)Record.StatusType.Running
                               select h).ToArray())
                h.Status = (int)Record.StatusType.Pending;
            int x=db.SaveChanges();
        }

        static ContestDaemon contest = new ContestDaemon();
        internal static List<AllKorrectDaemon> tester = new List<AllKorrectDaemon>();
        static SendMailDaemon email = new SendMailDaemon();
        static DispatcherDaemon dispatcher = new DispatcherDaemon();
        public static void DomainStart()
        {
            using (var db = new CHDB())
            {
                HuntBadWating(db);
                RecordBadWating(db);
                Group.AdministratorID = (from g in db.GROUPs
                                         where g.Name == "Administrators"
                                         select g.ID).Single();
            }
            contest.Start();
            foreach (var akinfo in AllKorrectDaemon.aks)
            {
                var ak = new AllKorrectDaemon()
                {
                    _ak = akinfo
                };
                ak.Start();
                tester.Add(ak);
            }
            dispatcher.Start();
            email.Start();
        }

        public static void DomainStop()
        {
            contest.Stop();
            foreach (var ak in tester)
            {
                ak.Stop();
            }
            dispatcher.Stop();
            email.Stop();
        }

        public static List<Daemon> DaemonList()
        {
            List<Daemon> ret = new List<Daemon>();
            ret.AddRange(tester);
            ret.Add(contest);
            ret.Add(email);
            ret.Add(dispatcher);
            return ret;
        }
    }
}