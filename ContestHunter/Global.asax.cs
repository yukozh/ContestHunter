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
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_AuthorizeRequest()
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    USER.Authenticate(User.Identity.Name);
                }
                catch (BadTokenException)
                {
                    FormsAuthentication.SignOut();
                    Response.Redirect("~/Shared/Error?msg=无效的用户凭据，请重新登录");
                    return;
                }
            }
        }
    }
}