using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using ContestHunter.Models.Domain;
using USER = ContestHunter.Models.Domain.User;
namespace ContestHunter
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        const string STR_503 = "<html><head><title>Slow Down Please</title></head><body><pre>"
            +"struct Hunter {\r\n"
            +"\tbool AreTired() {\r\n"
            +"\t\treturn true;\r\n"
            +"\t}\r\n"
            +"} you;\r\n"
            +"\r\n"
            + "int main() {\r\n"
            +"\tif (you.AreTired())\r\n"
            + "\t\tgoto 3839;\r\n"
            + "3839:\r\n"
            + "\t\"<a href='http://www.3839.com/'>http://www.3839.com/</a>\";\r\n"
            +"}"
            + "</pre></body></html>";


        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            Framework.WebRoot = Server.MapPath("~");
            Framework.DomainStart();
        }

        protected void Application_Stop()
        {
            Framework.DomainStop();
        }

        protected void Application_AuthorizeRequest()
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    USER.Authenticate(User.Identity.Name, Request.Headers["X-Forwarded-For"] ?? Request.UserHostAddress);
                }
                catch (BadTokenException)
                {
                    FormsAuthentication.SignOut();
                    Response.Redirect("~/Shared/Error?msg=无效的用户凭据，请重新登录");
                    return;
                }
            }
            try
            {
                //AccessRestriction.CheckRestriction(Request.Headers["X-Forwarded-For"] ?? Request.UserHostAddress);
            }
            catch
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.ServiceUnavailable;
                Response.Write(STR_503);
                Response.End();
                //Response.Redirect("http://www.3839.com/");
            }
        }
    }
}