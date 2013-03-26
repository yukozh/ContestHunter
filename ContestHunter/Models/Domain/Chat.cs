﻿using System;
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
        public static List<Message> GetCommon(DateTime Before,int top)
        {
            using (var db = new CHDB())
            {
                return (from r in db.MESSAGEs
                        where r.Session == Guid.Empty
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
        public static void PostCommon(Message Msg)
        {
            using (var db = new CHDB())
            {
                db.MESSAGEs.Add(new MESSAGE()
                {
                    Session = Guid.Empty,
                    Content = Msg.Content,
                    User = User.CurrentUser.ID,
                    Time = DateTime.Now,
                    ID = Guid.NewGuid()
                });
                db.SaveChanges();
            }
        }
    }
}