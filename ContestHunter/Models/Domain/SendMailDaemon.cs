using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContestHunter.Models.Domain
{
    public class SendMailDaemon : Daemon
    {
        public class Email
        {
            public string subject;
            public string to;
            public string content;
        }
        public static List<Email> EmailList = new List<Email>();
        protected override int Run()
        {
            if (EmailList.Count == 0)
                return 60000;
            Email email;
            lock (EmailList)
            {
                email = EmailList.Last();
                EmailList.RemoveAt(EmailList.Count - 1);
            }
            EmailHelper.Send(email.subject, email.to, email.content);
            return EmailList.Count > 0 ? 0 : 60000;
        }
    }
}