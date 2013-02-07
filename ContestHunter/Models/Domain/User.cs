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
        internal class OnlineUser
        {
            public Guid ID;
            public Guid Token;
            public string name;
        }

        static Dictionary<Guid,OnlineUser> OnlineUsers=new Dictionary<Guid,OnlineUser>();

        [ThreadStatic]
        static internal OnlineUser CurrentUser;

        /// <summary>
        /// 验证Token
        /// </summary>
        /// <param name="Token"></param>
        /// <exception cref="BadTokenException"></exception>
        public static void Authenticate(string Token)
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
        public static void SendValidationEmail(string name, string password, string email,string url)
        {
            SmtpClient client = new SmtpClient(); 
            client.Credentials = new System.Net.NetworkCredential("hellotyvj@gmail.com", "07070078899");
            client.Port = 587;
            client.Host = "smtp.gmail.com"; 
            client.EnableSsl = true;
            client.Send("Register@ContestHunter.com", email, "ContestHunter注册验证", ""+
                url+"?name="+HttpUtility.UrlEncode(name)+
                "&password="+HttpUtility.UrlEncode(DESHelper.Encrypt(Encoding.Unicode.GetBytes(password)))+
                "&email="+HttpUtility.UrlEncode(name)
                );
        }
        /// <summary>
        /// 验证密码并添加用户
        /// </summary>
        /// <param name="name"></param>
        /// <param name="originalPassword"></param>
        /// <param name="encryptedPassword"></param>
        /// <param name="email"></param>
        /// <exception cref="PasswordMismatchException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public static void Register(string name, string originalPassword, string encryptedPassword, string email)
        {
            if (originalPassword != Encoding.Unicode.GetString(DESHelper.Decrypt(HttpUtility.UrlDecodeToBytes(encryptedPassword))))
                throw new PasswordMismatchException();
            using (var db = new CHDB())
            {
                using (SHA256 hash = SHA256.Create())
                {
                    db.USERs.Add(new USER()
                    {
                        ID=Guid.NewGuid(),
                        Name = name,
                        Password = hash.ComputeHash(Encoding.Unicode.GetBytes(originalPassword)),
                        Email = email
                    });
                }
                if (1 != db.SaveChanges())
                    throw new DatabaseException();
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
                return (from u in db.USERs where u.Name == name select u).Any();
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
        public static string Login(string name, string password)
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
                    if (!Enumerable.SequenceEqual<byte>(pwdInBytes,currentUser.Password))
                        throw new PasswordMismatchException();
                    var newToken = new OnlineUser()
                        {
                            ID = currentUser.ID,
                            Token = Guid.NewGuid(),
                            name = currentUser.Name
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
            throw new UndefineException();
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
            }
        }
    }
}