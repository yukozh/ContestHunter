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
            return Chat.GetHistory(Chat.CommonSession, before, top).Select(x =>
            {
                RatingInfo rating = new RatingInfo(x.Username);
                return new
                {
                    Content = x.Content,
                    Time = x.Time.ToUniversalTime(),
                    User = x.Username,
                    UserImg = Gravatar.GetAvatarURL(USER.ByName(x.Username).Email, 50),
                    UserColor = rating.Color,
                    UserCaption = rating.Caption
                };
            });
        }

        public void Post(Chat.Message msg)
        {
            Chat.PostMessage(new Chat.Session() { ID = Guid.Empty }, msg);

            var notify = MyWebSocket.JSON.Serialize(new
            {
                Type = "CommonMessage",
                Message = new
                {
                    Content = msg.Content,
                    Time = DateTime.UtcNow.ToString("o"),
                    User = USER.CurrentUserName,
                    UserImg = Gravatar.GetAvatarURL(USER.ByName(USER.CurrentUserName).Email, 50)
                }
            });

            MyWebSocket.Broadcast(notify).Wait();
        }
    }
}
