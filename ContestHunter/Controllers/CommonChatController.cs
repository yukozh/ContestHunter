using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ContestHunter.Models.Domain;
using ContestHunter.ViewHelpers;
using USER = ContestHunter.Models.Domain.User;
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
                User = x.Username,
                UserImg = Gravatar.GetAvatarURL(USER.ByName(x.Username).Email,50)
            });
        }

        public void Post(Chat.Message msg)
        {
            Chat.PostCommon(msg);
        }
    }
}
