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

        public class Session
        {
            public Guid ID;
            public string Name;
            public SessionType Type;
        }

        public enum SessionType
        {
            CommonChat,
            GroupChat,
            PrivateChat
        };


        /// <summary>
        /// 获取公共聊天区内容
        /// </summary>
        /// <param name="Before"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public static List<Message> GetHistory(Session Session, DateTime Before, int top)
        {
            using (var db = new CHDB())
            {
                return (from r in db.MESSAGEs
                        where r.Session == Session.ID
                        && r.Time < Before
                        orderby r.Time descending
                        select new Message()
                        {
                            Content = r.Content,
                            Time = r.Time,
                            Username = r.USER1.Name
                        }).Take(top).ToList();
            }
        }

        /// <summary>
        /// 发送消息至公共聊天区 只需要填充 Content
        /// </summary>
        /// <param name="Msg"></param>
        public static void PostMessage(Session Session, Message Msg)
        {
            using (var db = new CHDB())
            {
                db.MESSAGEs.Add(new MESSAGE()
                {
                    Session = Session.ID,
                    Content = Msg.Content,
                    User = User.CurrentUser.ID,
                    Time = DateTime.Now,
                    ID = Guid.NewGuid()
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取公共聊天区会话信息
        /// </summary>
        /// <returns></returns>
        public static Session CommonSession = new Session() { ID = Guid.Empty };

        /// <summary>
        /// 获取私聊会话
        /// </summary>
        /// <param name="name">要私聊用户名称</param>
        /// <returns></returns>
        public static Session GetPrivateSession(string name)
        {
            using (var db = new CHDB())
            {
                var u1 = (from u in db.USERs
                          where u.Name == name
                          select u).Single();
                var u2 = (from u in db.USERs
                          where u.ID == User.CurrentUser.ID
                          select u).Single();
                var ret = (from s in u2.SESSIONs
                           where s.USERs.Select(x => x.Name).Contains(name)
                           && s.Type == (int)SessionType.PrivateChat
                           select new Session()
                                     {
                                         ID = s.ID,
                                         Name = s.Name,
                                         Type = SessionType.PrivateChat
                                     }).FirstOrDefault();
                if (null == ret)
                {
                    var session = db.SESSIONs.Add(new SESSION()
                    {
                        ID = Guid.NewGuid(),
                        Name = "",
                        Type = (int)SessionType.PrivateChat
                    });
                    session.USERs.Add(u1);
                    session.USERs.Add(u2);
                    db.SaveChanges();
                    return new Session()
                    {
                        ID = session.ID,
                        Type = SessionType.PrivateChat
                    };
                }
                return ret;
            }
        }
    }
}