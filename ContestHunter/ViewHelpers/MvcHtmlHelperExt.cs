using System;
using System.Text;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
namespace ContestHunter.ViewHelpers
{
    public static class MvcHtmlHelperExt
    {
        public static string ToHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach(byte  b in bytes){
                string tmp=Convert.ToString(b, 16);
                if (tmp.Length == 1) sb.Append('0').Append(tmp);
                else sb.Append(tmp);
            }
            return sb.ToString();
        }
        public static MvcHtmlString Sanitized<TModel>(this HtmlHelper<TModel> self, string html)
        {
            if (html == null) return new MvcHtmlString("");
            return new MvcHtmlString(HtmlFilter.Instance.SanitizeHtml(html));
        }

        public static MvcHtmlString Gravatar<TModel>(this HtmlHelper<TModel> self, string email, int size)
        {
            string md5 = ToHexString(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(email)));
            string url = string.Format("http://www.gravatar.com/avatar/{0}?s={1}&d=mm", md5, size);
            string tag = string.Format("<img src='{0}' style='width:{1}px;height:{1}px;' />", HttpUtility.HtmlAttributeEncode(url), size);
            return new MvcHtmlString(tag);
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

            return MvcHtmlString.Create(string.Format(@"<span{0}>{1}</span>", sb.ToString(), description));
        }
    }
}