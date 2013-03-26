using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContestHunter.Database;

namespace ContestHunter.Models.Domain
{
    public class Chat
    {
        public class Message
        {
            public string Username;
            public string Content;
            public DateTime Time;
        }

        public static List<Message> GetCommon(DateTime Before,int top)
        {
            using (var db = new CHDB())
            {
                return (from r in db.CHAT_COMMON
                        where r.Time < Before
                        select new Message()
                        {
                            Content = r.Content,
                            Time = r.Time,
                            Username = r.USER1.Name
                        }).Take(top).ToList();
            }
        }

        public static void PostCommon(Message Msg)
        {
            using (var db = new CHDB())
            {
                db.CHAT_COMMON.Add(new CHAT_COMMON()
                {
                    ID = Guid.NewGuid(),
                    Content = Msg.Content,
                    User = User.CurrentUser.ID,
                    Time = DateTime.Now
                });
                db.SaveChanges();
            }
        }
    }
}