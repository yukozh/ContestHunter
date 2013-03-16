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
        const string STR_503 = "<html><head><title>手速太快了</title></head><body>"
            + "<style>"
                + ".m{font-size:80px;}"
                + ".l{font-size:100px;}"
                + ".xl{font-size:150px;}"
                + ".y{font-family:微软雅黑;}"
            + "</style>"
            + "<div style=\"text-align:center;\">"
                + "<div class=\"l\">别<span class=\"xl y\">刷</span>了</div>"
                + "<div class=\"m\">越<span class=\"l y\">刷</span>越<span class=\"l y\">慢</span></div>"
                + "<div class=\"m\"><span class=\"l y\">再</span>刷<span class=\"l y\">关</span>站</div>"
            + "</div>"
            + "</body></html>";


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
                AccessRestriction.CheckRestriction(Request.Headers["X-Forwarded-For"] ?? Request.UserHostAddress);
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