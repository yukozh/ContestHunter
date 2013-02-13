using System;
using System.Text;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
namespace ContestHunter.ViewHelpers
{
    public static class MvcHtmlHelperExt
    {
        public static MvcHtmlString Sanitized<TModel>(this HtmlHelper<TModel> self, string html)
        {
            if (html == null) return new MvcHtmlString("");
            return new MvcHtmlString(HtmlFilter.Instance.SanitizeHtml(html));
        }

        public static MvcHtmlString UserLink<TModel>(this HtmlHelper<TModel> self, string userName)
        {
            return self.ActionLink(userName, "Show", "User", new { id = userName }, null);
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

            return MvcHtmlString.Create(string.Format(@"<p{0}>{1}</p>", sb.ToString(), description));
        }
    }
}