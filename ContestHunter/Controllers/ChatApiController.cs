using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ContestHunter.Models.Domain;
namespace ContestHunter.Controllers
{
    public class ChatApiController : ApiController
    {
        public IEnumerable<Chat.Message> GetCommon(DateTime before, int top)
        {
            return Chat.GetCommon(before, top);
        }
    }
}
