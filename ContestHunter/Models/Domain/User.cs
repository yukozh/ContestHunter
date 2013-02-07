using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace ContestHunter.Models.Domain
{
    public class User
    {
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
        public static void Register(string name, string originalPassword, string encryptedPassword, string email)
        {

        }
    }
}