﻿using System.Web;
using System.Web.Mvc;

namespace ContestHunter
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AuthorizeAttribute());
            filters.Add(new ValidateInputAttribute(false));
        }
    }
}