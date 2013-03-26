using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ContestHunter.Models.Domain;
namespace ContestHunter.Controllers
{
    [Authorize]
    public class CommonChatController : ApiController
    {
        [AllowAnonymous]
        public object Get(DateTime before, int top)
        {
            return Chat.GetCommon(before, top).Select(x => new
            {
                Content = x.Content,
                Time = x.Time.ToUniversalTime(),
                User = x.Username
            });
        }

        public Chat.Message Post(Chat.Message msg)
        {
            Chat.PostCommon(msg);
            return msg;
        }
    }
}
