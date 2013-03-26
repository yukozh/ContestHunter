using System;
using System.Text;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using ContestHunter.Models.Domain;
namespace ContestHunter.ViewHelpers
{
    public static class MvcHtmlHelperExt
    {
        
        public static MvcHtmlString Sanitized<TModel>(this HtmlHelper<TModel> self, string html)
        {
            if (html == null) return new MvcHtmlString("");
            return new MvcHtmlString(HtmlFilter.Instance.SanitizeHtml(html));
        }

        public static MvcHtmlString Gravatar<TModel>(this HtmlHelper<TModel> self, string email, int size)
        {
            string url = ContestHunter.ViewHelpers.Gravatar.GetAvatarURL(email, size);
            string tag = string.Format("<img src='{0}' style='width:{1}px;height:{1}px;' />", HttpUtility.HtmlAttributeEncode(url), size);
            return new MvcHtmlString(tag);
        }

        public static MvcHtmlString UserLink<TModel>(this HtmlHelper<TModel> self, string userName)
        {
            RatingInfo info = new RatingInfo(userName);

            return self.ActionLink(userName, "Show", "User", new { id = userName }, new { title = info.Caption, style = "color: " + info.Color });
        }

        public static MvcHtmlString UserLink<TModel>(this HtmlHelper<TModel> self, string userName, string action, string controller, object routeValues)
        {
            RatingInfo info = new RatingInfo(userName);

            return self.ActionLink(userName, action, controller, routeValues, new { title = info.Caption, style = "color: " + info.Color });
        }

        public static MvcHtmlString UserLink<TModel>(this HtmlHelper<TModel> self, string userName, object htmlAttr)
        {
            return self.ActionLink(userName, "Show", "User", new { id = userName }, htmlAttr);
        }

        public static MvcHtmlString DescriptionFor<TModel, TValue>(this HtmlHelper<TModel> self, Expression<Func<TModel, TValue>> expression)
        {
            return DescriptionFor(self, expression, new object());
        }
        public static MvcHtmlString DescriptionFor<TModel, TValue>(this HtmlHelper<TModel> self, Expression<Func<TModel, TValue>> expression, object htmlAttrbutes)
        {
            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttrbutes);
            StringBuilder sb = new StringBuilder();
            foreach (var attr in attributes)
            {
                sb.Append(' ').Append(attr.Key).Append("=\"").Append(HttpUtility.HtmlAttributeEncode(attr.Value.ToString())).Append('\"');
            }


            var metadata = ModelMetadata.FromLambdaExpression(expression, self.ViewData);
            var description = metadata.Description;

            return MvcHtmlString.Create(string.Format(@"<span{0}>{1}</span>", sb.ToString(), description));
        }

        public static RouteValueDictionary DisabledIf(this object htmlAttributes, bool disabled)
        {
            var attributes = new RouteValueDictionary(htmlAttributes);
            if (disabled)
            {
                attributes["disabled"] = "disabled";
            }
            return attributes;
        }
    }
}