using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Net.WebSockets;
using Microsoft.Web.WebSockets;
using ContestHunter.ViewHelpers;
using ContestHunter.Models.Domain;
using USER = ContestHunter.Models.Domain.User;
namespace ContestHunter.Controllers
{
    [Authorize]
    public class WebSocketController : ApiController
    {
        public HttpResponseMessage Get()
        {
            HttpContext.Current.AcceptWebSocketRequest(new MyWebSocket() { User = USER.CurrentUserName });
            return new HttpResponseMessage(HttpStatusCode.SwitchingProtocols);
        }

        [AllowAnonymous]
        public IEnumerable<object> GetOnlineList(string onlineList)
        {
            return MyWebSocket.OnlineList.Select(u =>
            {
                RatingInfo rating = new RatingInfo(u);
                return new
                {
                    User = u,
                    UserColor = rating.Color,
                    UserCaption = rating.Caption
                };
            });
        }
    }

    class MyWebSocket : WebSocketHandler
    {
        [ThreadStatic]
        static JavaScriptSerializer json;

        public static JavaScriptSerializer JSON
        {
            get
            {
                if (json == null) json = new JavaScriptSerializer();
                return json;
            }
        }

        static Dictionary<string, MyWebSocket> Clients = new Dictionary<string, MyWebSocket>();
        public string User;
        bool noRemove;

        public override void OnOpen()
        {
            Task toWait = null;
            bool success = true;
            lock (Clients)
            {
                if (Clients.ContainsKey(User))
                {
                    if (WebSocketContext.QueryString["forceLogin"] == "true")
                    {
                        var oldConn = Clients[User];
                        oldConn.noRemove = true;
                        toWait = Task.Run(() =>
                        {
                            oldConn.Send(JSON.Serialize(new { Type = "AnotherLogin" }));
                            oldConn.Close();
                        });

                        Clients[User] = this;
                    }
                    else
                    {
                        toWait = Task.Run(() =>
                        {
                            Send(JSON.Serialize(new { Type = "AlreadyLogin" }));
                            Close();
                        });
                        success = false;
                    }
                }
                else
                {
                    Clients.Add(User, this);
                }
            }
            if (toWait != null)
                toWait.Wait();
            //Broadcast
            var rating = new RatingInfo(User);
            Broadcast(JSON.Serialize(new
            {
                Type = "Login",
                User = User,
                UserColor = rating.Color,
                UserCaption = rating.Caption
            })).Wait();
        }
        public override void OnClose()
        {
            Task toWait = null;
            lock (Clients)
            {
                if (Clients.ContainsKey(User))
                {
                    if (!noRemove)
                    {
                        Clients.Remove(User);
                    }
                    //Broadcast
                    toWait = Broadcast(JSON.Serialize(new { Type = "Logout", User = User }));
                }
            }
            toWait.Wait();
        }
        public override void OnMessage(string message)
        {
            base.OnMessage(message);
        }

        public static async Task Broadcast(string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            var toWait = Clients.Values.Select(c =>
                c.WebSocketContext.WebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, System.Threading.CancellationToken.None)
            );
            await Task.WhenAll(toWait);
        }

        public static IEnumerable<string> OnlineList
        {
            get
            {
                return Clients.Keys;
            }
        }
    }
}
