using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Security.Principal;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using ContestHunter.Database;
using System.Threading;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Collections.Concurrent;

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
        public string Motto;
        public bool AcceptEmail;
        internal int _isAdmin = -1;
        public bool IsAdmin
        {
            get
            {
                if (_isAdmin == -1)
                {
                    using (var db = new CHDB())
                    {
                        return 0 < (_isAdmin = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM [USER_GROUP] WHERE [User]=@uid and [Group]=@gid", new SqlParameter("uid", ID), new SqlParameter("gid", Group.AdministratorID)).Single());
                    }
                }
                return _isAdmin > 0;
            }
            set
            {
                _isAdmin = value ? 1 : 0;
            }
        }
        public int Rating;

        internal Guid ID;
        internal class OnlineUser : IIdentity
        {
            public Guid ID;
            public Guid Token;
            public string name;
            public string email;
            public string ip;
            public List<string> groups;
            public bool IsAdmin;

            public string AuthenticationType
            {
                get { return "CHCustom"; }
            }

            public bool IsAuthenticated
            {
                get { return true; }
            }

            public string Name
            {
                get { return name; }
            }
        }

        class CustomPriciple : IPrincipal
        {
            public OnlineUser TheUser;
            public IIdentity Identity
            {
                get { return TheUser; }
            }

            public bool IsInRole(string role)
            {
                return TheUser.groups.Contains(role);
            }
        }

        static ConcurrentDictionary<Guid, OnlineUser> OnlineUsers = new ConcurrentDictionary<Guid, OnlineUser>();

        static internal OnlineUser CurrentUser
        {
            get
            {
                IPrincipal principal = Thread.CurrentPrincipal;
                if (!(principal is CustomPriciple)) return null;
                return (OnlineUser)principal.Identity;
            }
            set
            {
                if (value == null)
                {
                    Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(""), new string[0]);
                }
                else
                {
                    Thread.CurrentPrincipal = new CustomPriciple() { TheUser = value };
                }
                if (null != HttpContext.Current)
                {
                    HttpContext.Current.User = Thread.CurrentPrincipal;
                }
            }
        }

        static public string CurrentUserName
        {
            get
            {
                if (null == CurrentUser)
                    throw new UserNotLoginException();
                return CurrentUser.name;
            }
        }

        static public bool CurrentUserIsAdmin
        {
            get
            {
                if (null == CurrentUser)
                    return false;
                return CurrentUser.IsAdmin;
            }
        }

        static public Record.LanguageType? CurrentUserPreferLanguage
        {
            get
            {
                if (null == CurrentUser)
                    throw new UserNotLoginException();
                using (var db = new CHDB())
                {
                    return (Record.LanguageType?)(from u in db.USERs
                                                  where u.ID == CurrentUser.ID
                                                  select u.PreferLanguage).Single();
                }
            }
        }

        /// <summary>
        /// 验证Token
        /// </summary>
        /// <param name="Token"></param>
        /// <exception cref="BadTokenException"></exception>
        public static void Authenticate(string Token, string ip)
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
            email = email.Trim().ToLower();
            string emailHash = Convert.ToBase64String(DESHelper.Encrypt(Encoding.UTF8.GetBytes(email)));

            url += "?name=" + HttpUtility.UrlEncode(name);
            url += "&password=" + HttpUtility.UrlEncode(encryptedPassword);
            url += "&email=" + HttpUtility.UrlEncode(email);
            url += "&emailHash=" + HttpUtility.UrlEncode(emailHash);

            string link = string.Format("<a href='{0}'>{1}</a>", HttpUtility.HtmlAttributeEncode(url), HttpUtility.HtmlEncode(url));

            EmailHelper.Send("ContestHunter注册验证", email, "请访问 " + link + " 完成注册");
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
            string country, string province, string city, string school, string motto, string realName)
        {
            email = email.Trim().ToLower();
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
                        Name = Helper.GetLegalName(name),
                        Password = hash.ComputeHash(Encoding.Unicode.GetBytes(originalPassword)),
                        Email = email,
                        Country = country,
                        Province = province,
                        City = city,
                        School = school,
                        Motto = motto,
                        RealName = realName,
                        AcceptEmail = true
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
        public static string Login(string name, string password, string ip)
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
                    currentUser.LastLoginIP = ip;
                    currentUser.LastLoginTime = DateTime.Now;
                    var newToken = new OnlineUser()
                        {
                            ID = currentUser.ID,
                            Token = Guid.NewGuid(),
                            name = currentUser.Name,
                            email = currentUser.Email,
                            groups = (from g in currentUser.GROUPs
                                      select g.Name).ToList(),
                            ip = ip,
                            IsAdmin = currentUser.GROUPs.Where(x => x.Name == "Administrators").Any()
                        };
                    OnlineUsers.AddOrUpdate(currentUser.ID, newToken, (Guid id, OnlineUser oldv) => newToken);
                    db.SaveChanges();
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
            OnlineUser tmp;
            OnlineUsers.TryRemove(CurrentUser.ID, out tmp);
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
                    Motto = result.Motto,
                    RealName = result.RealName,
                    School = result.School,
                    LastLoginTime = privillege ? result.LastLoginTime : null,
                    LastLoginIP = privillege ? result.LastLoginIP : null,
                    AcceptEmail = result.AcceptEmail,
                    Rating = result.Rating ?? 0
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
                    if (!hash.ComputeHash(Encoding.Unicode.GetBytes(oriPassword)).SequenceEqual(usr.Password))
                        throw new PasswordMismatchException();
                    if (null != Password)
                        usr.Password = hash.ComputeHash(Encoding.Unicode.GetBytes(Password));
                    usr.Country = Country;
                    usr.Province = Province;
                    usr.City = City;
                    usr.School = School;
                    usr.Email = Email;
                    usr.RealName = RealName;
                    usr.Motto = Motto;
                    usr.AcceptEmail = AcceptEmail;
                    db.SaveChanges();
                }
            }
        }
        

        /// <summary>
        /// 按照 Rating 排名返回用户列表
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public static List<User> List(int skip, int top)
        {
            using (var db = new CHDB())
            {
                return (from u in db.RankLists
                        orderby u.Rating descending
                        select new User()
                        {
                            Name = u.Name,
                            ID = u.ID,
                            Motto = u.Motto,
                            Rating = u.Rating ?? 0,
                            _isAdmin = u.IsAdmin
                        }).Skip(skip).Take(top).ToList();
            }
        }

        /// <summary>
        /// 获得当前用户的名次
        /// </summary>
        /// <returns></returns>
        public int Rank()
        {
            using (var db = new CHDB())
            {
                return (from u in db.USERs
                        let rating = u.RATINGs.OrderByDescending(x => x.CONTEST1.EndTime).Select(x => x.Rating1).FirstOrDefault()
                        where rating > Rating
                        select u).Count();
            }
        }

        /// <summary>
        /// 获得用户所有Rating记录
        /// </summary>
        /// <returns></returns>
        public List<Rating> RatingHistory()
        {
            using (var db = new CHDB())
            {
                return (from r in db.RATINGs
                        where r.USER1.ID == ID
                        orderby r.CONTEST1.EndTime ascending
                        select new Rating()
                        {
                            Score = r.Rating1,
                            Time = r.CONTEST1.EndTime,
                            Contest = r.CONTEST1.Name
                        }).ToList();
            }
        }

        /// <summary>
        /// 获得用户总数
        /// </summary>
        /// <returns></returns>
        public static int Count()
        {
            using (var db = new CHDB())
            {
                return (from u in db.USERs
                        select u).Count();
            }
        }
    }
}