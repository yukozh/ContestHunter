using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using ContestHunter.Models.View;
using ContestHunter.Models.Domain;
using USER = ContestHunter.Models.Domain.User;
namespace ContestHunter.Controllers
{
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            HomeIndexModel model = new HomeIndexModel
            {
                Rating = USER.List(0, 10),
                News = ConfigurationManager.AppSettings["News"]
            };
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult BadBrowser()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Process()
        {
            return View(Framework.DaemonList());
        }

        #region Captcha
        static string get()
        {
            using (WebClient client = new WebClient())
            {
                //string html = Encoding.UTF8.GetString(client.DownloadData("http://wapp.baidu.com/m?kw=wow&pn=" + new Random().Next(10000)));
                //var matches = Regex.Matches(html, @"<div class=""i""><a .*?>\d*\.&#160;(.*?)</a>");
                string html = Encoding.UTF8.GetString(client.DownloadData("http://www.guokr.com/group/30/?page=" + new Random().Next(300)));
                var matches = Regex.Matches(html, @"<h3 class=""titles-txt""><a .*?>(.*?)\s*</a>",RegexOptions.Singleline);
                foreach (Match match in matches)
                {
                    return HttpUtility.HtmlDecode(match.Groups[1].Value);
                }
                return null;
            }
        }

        public static void Shuffle<T>(IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        static List<KeyValuePair<string, int>> GetCaptcha()
        {
            const int PART = 6;
            string s;
            do
            {
                s = get();
            } while (s.Length < PART);
            Random rnd = new Random();


            var poses = new List<int>();
            for (int i = 0; i < PART - 1; i++)
            {
                int cur = rnd.Next(s.Length - 1 - i) + 1;
                for (int j = 0; j < i; j++)
                {
                    if (poses[j] <= cur) cur++;
                }
                poses.Add(cur);
                poses.Sort();
            }
            int last = 0;
            var subs = new List<KeyValuePair<string,int>>();
            foreach (var pos in poses)
            {
                subs.Add(new KeyValuePair<string,int>(s.Substring(last, pos - last),0));
                last = pos;
            }
            subs.Add(new KeyValuePair<string,int>(s.Substring(last),0));
            for (int i = 0; i < subs.Count; i++)
                subs[i] = new KeyValuePair<string, int>(subs[i].Key, i);
            Shuffle(subs);
            return subs;
        }

        static Dictionary<Guid, int[]> Answers = new Dictionary<Guid, int[]>();

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Captcha()
        {
            var captcha=GetCaptcha();
            int[] answer = new int[captcha.Count];
            for (int i = 0; i < captcha.Count; i++)
            {
                answer[captcha[i].Value] = i;
            }
            ViewBag.ID = Guid.NewGuid();
            Answers.Add(ViewBag.ID, answer);
            return View(captcha.Select(i => i.Key).ToList());
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Captcha(Guid id, string answer)
        {
            ModelState.Clear();
            if (Answers.ContainsKey(id))
            {
                if (answer == string.Join("|", Answers[id]) + "|")
                {
                    ViewBag.Last = "yes";
                }
                else
                {
                    ViewBag.Last = "no";
                }
                Answers.Remove(id);
            }
            return Captcha();
        }
        #endregion
    }
}
