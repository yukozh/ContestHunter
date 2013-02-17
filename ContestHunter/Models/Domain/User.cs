using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using ContestHunter.Database;
using System.Threading;

namespace ContestHunter.Models.Domain
{
    public class User
    {

        public string Name;
        public string Email;
        public string Country;
        public string Province;
        public string City;
        public string School;
        public string RealName;
        public string LastLoginIP;
        public DateTime? LastLoginTime;
        public string Password;
        public string Matto;

        internal Guid ID;
        internal class OnlineUser
        {
            public Guid ID;
            public Guid Token;
            public string name;
            public string email;
            public string ip;
            public List<string> groups;
        }

        static Dictionary<Guid, OnlineUser> OnlineUsers = new Dictionary<Guid, OnlineUser>();

        [ThreadStatic]
        static internal OnlineUser CurrentUser;

        static public string CurrentUserName
        {
            get 
            {
                if (null == CurrentUser)
                    throw new UserNotLoginException();
                return CurrentUser.name; 
            }
        }

        /// <summary>
        /// 验证Token
        /// </summary>
        /// <param name="Token"></param>
        /// <exception cref="BadTokenException"></exception>
        public static void Authenticate(string Token,string ip)
        {
            lock (OnlineUsers)
            {
                string[] tokens = Token.Split(new char[] { '|' });
                if (tokens.Length != 2)
                    throw new BadTokenException();
                Guid uid = Guid.Parse(tokens[0]), tk = Guid.Parse(tokens[1]);
                if (!OnlineUsers.ContainsKey(uid))
                    throw new BadTokenException();
                if (OnlineUsers[uid].Token != tk)
                    throw new BadTokenException();
                if (OnlineUsers[uid].ip != ip)
                    throw new BadTokenException();
                CurrentUser = OnlineUsers[uid];
            }
        }

        /// <summary>
        /// 发送验证邮件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <param name="url"></param>
        public static void SendValidationEmail(string name, string password, string email, string url)
        {
            string encryptedPassword = Convert.ToBase64String(DESHelper.Encrypt(Encoding.UTF8.GetBytes(password)));
            string emailHash = Convert.ToBase64String(DESHelper.Encrypt(Encoding.UTF8.GetBytes(email.Trim().ToLower())));

            url += "?name=" + HttpUtility.UrlEncode(name);
            url += "&password=" + HttpUtility.UrlEncode(encryptedPassword);
            url += "&email=" + HttpUtility.UrlEncode(email);
            url += "&emailHash=" + HttpUtility.UrlEncode(emailHash);

            string link = string.Format("<a href='{0}'>{1}</a>", HttpUtility.HtmlAttributeEncode(url), HttpUtility.HtmlEncode(url));

            using (MailMessage msg = new MailMessage())
            {
                msg.From = new MailAddress("Register@ContestHunter.com");
                msg.To.Add(email);
                msg.Subject = "ContestHunter注册验证";
                msg.Body = "请访问 " + link + " 完成注册";
                msg.IsBodyHtml = true;

                SmtpClient client = new SmtpClient();
                client.Credentials = new System.Net.NetworkCredential("hellotyvj@gmail.com", "07070078899");
                client.Port = 587;
                client.Host = "smtp.gmail.com";
                client.EnableSsl = true;
                client.Send(msg);
            }
        }

        /// <summary>
        /// 验证密码并添加用户
        /// </summary>
        /// <param name="name"></param>
        /// <param name="originalPassword"></param>
        /// <param name="encryptedPassword"></param>
        /// <param name="email"></param>
        /// <exception cref="PasswordMismatchException"></exception>
        /// <exception cref="EmailMismatchException"></exception>
        public static void Register(string name, string originalPassword, string encryptedPassword, string email, string emailHash,
            string country,string province,string city,string school,string matto,string realName)
        {
            if (originalPassword != Encoding.UTF8.GetString(DESHelper.Decrypt(Convert.FromBase64String(encryptedPassword))))
                throw new PasswordMismatchException();
            if (email != Encoding.UTF8.GetString(DESHelper.Decrypt(Convert.FromBase64String(emailHash))))
                throw new EmailMismatchException();

            using (var db = new CHDB())
            {
                using (SHA256 hash = SHA256.Create())
                {
                    db.USERs.Add(new USER()
                    {
                        ID = Guid.NewGuid(),
                        Name = name,
                        Password = hash.ComputeHash(Encoding.Unicode.GetBytes(originalPassword)),
                        Email = email,
                        Country = country,
                        Province = province,
                        City = city,
                        School = school,
                        Matto = matto,
                        RealName = realName
                    });
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 返回指定用户名是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsNameExisted(string name)
        {
            using (var db = new CHDB())
            {
                return (from u in db.USERs
                        where u.Name == name
                        select u).Any();
            }
        }

        /// <summary>
        /// 返回指定的Email是否存在
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsEmailExisted(string email)
        {
            using (var db = new CHDB())
            {
                return (from u in db.USERs
                        where u.Email == email
                        select u).Any();
            }
        }

        /// <summary>
        /// 登陆帐户
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="UserNotFoundException"></exception>
        /// <exception cref="PasswordMismatchException"></exception>
        public static string Login(string name, string password,string ip)
        {

            using (var sha = SHA256.Create())
            {
                byte[] pwdInBytes = sha.ComputeHash(Encoding.Unicode.GetBytes(password));
                using (var db = new CHDB())
                {
                    var currentUser = (from u in db.USERs
                                       where u.Name == name
                                       select u).SingleOrDefault();
                    if (null == currentUser)
                        throw new UserNotFoundException();
                    if (!Enumerable.SequenceEqual<byte>(pwdInBytes, currentUser.Password))
                        throw new PasswordMismatchException();
                    var newToken = new OnlineUser()
                        {
                            ID = currentUser.ID,
                            Token = Guid.NewGuid(),
                            name = currentUser.Name,
                            email = currentUser.Email,
                            groups = (from g in currentUser.GROUPs
                                      select g.Name).ToList(),
                            ip = ip
                        };
                    lock (OnlineUsers)
                    {
                        if (OnlineUsers.ContainsKey(currentUser.ID))
                            OnlineUsers[currentUser.ID] = newToken;
                        else
                            OnlineUsers.Add(currentUser.ID, newToken);
                    }

                    return currentUser.ID.ToString() + "|" + newToken.Token.ToString();
                }
            }
            throw new UndefinedException();
        }

        /// <summary>
        /// 注销帐户
        /// </summary>
        /// <param name="token"></param>
        public static void Logout()
        {
            lock (OnlineUsers)
            {
                OnlineUsers.Remove(CurrentUser.ID);
                CurrentUser = null;
            }
        }

        /// <summary>
        /// 获得用户信息
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="UserNotFoundException"></exception>
        public static User ByName(string name)
        {
            using (var db = new CHDB())
            {
                var result = (from u in db.USERs
                              where u.Name == name
                              select u).SingleOrDefault();
                if (null == result)
                    throw new UserNotFoundException();
                bool privillege = false;
                if (null != CurrentUser && result.Name == CurrentUser.name)
                    privillege = true;
                return new User()
                {
                    Name = result.Name,
                    Email = result.Email,
                    ID = result.ID,
                    Country = result.Country,
                    Province = result.Province,
                    City = result.City,
                    Matto = result.Matto,
                    RealName = result.RealName,
                    School = result.School,
                    LastLoginTime = privillege ? result.LastLoginTime : null,
                    LastLoginIP = privillege ? result.LastLoginIP : null
                };
            }
        }

        /// <summary>
        /// 返回用户所有用户组
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PermissionDeniedException"></exception>
        public List<Group> Groups()
        {
            Group.CheckPriviledge();
            using (var db = new CHDB())
            {
                return (from g in
                            (from u in db.USERs
                             where u.Name == Name
                             select u.GROUPs).Single()
                        select new Group
                        {
                            Name = g.Name,
                            ID = g.ID
                        }).ToList();
            }
        }

        /*
        public override bool Equals(object obj)
        {
            if (obj is User)
                return Name == ((User)obj).Name;
            return base.Equals(obj);
        }
         * */

        /// <summary>
        /// 修改用户信息，必须验证当前密码
        /// </summary>
        /// <param name="oriPassword"></param>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="PermissionDeniedException"></exception>
        /// <exception cref="PasswordMismatchException"></exception>
        public void Change(string oriPassword)
        {
            if (null == CurrentUser)
                throw new UserNotLoginException();
            if (CurrentUser.ID != ID)
                throw new PermissionDeniedException();
            using (SHA256 hash = SHA256.Create())
            {
                using (var db = new CHDB())
                {
                    var usr = (from u in db.USERs
                               where u.ID == ID
                               select u).Single();
                    if (!hash.ComputeHash(Encoding.Unicode.GetBytes(oriPassword)).Equals(usr.Password))
                        throw new PasswordMismatchException();
                    if (null != Password)
                        usr.Password = hash.ComputeHash(Encoding.Unicode.GetBytes(Password));
                    usr.Country = Country;
                    usr.Province = Province;
                    usr.City = City;
                    usr.School = School;
                    usr.Email = Email;
                    usr.RealName = RealName;
                    usr.Matto = Matto;
                    db.SaveChanges();
                }
            }
        }
    }
}