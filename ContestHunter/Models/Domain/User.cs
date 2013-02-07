using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;

namespace ContestHunter.Models.Domain
{
    public class User
    {
        public static void SendValidationEmail(string name, string password, string email)
        {
            MailMessage msg = new System.Net.Mail.MailMessage(); 
            msg.To.Add(email);
            msg.From = new MailAddress("Register@ContestHunter.com", "Test", System.Text.Encoding.UTF8); 
            msg.Subject = "这是测试邮件";//邮件标题 
            msg.SubjectEncoding = System.Text.Encoding.UTF8;//邮件标题编码 
            msg.Body = "邮件内容";//邮件内容 
            msg.BodyEncoding = System.Text.Encoding.UTF8;//邮件内容编码 
            msg.IsBodyHtml = false;//是否是HTML邮件 
            msg.Priority = MailPriority.High;//邮件优先级 
            SmtpClient client = new SmtpClient(); 
            client.Credentials = new System.Net.NetworkCredential("hellotyvj@gmail.com", "07070078899"); 
            //上述写你的GMail邮箱和密码 
            client.Port = 587;//Gmail使用的端口 
            client.Host = "smtp.gmail.com"; 
            client.EnableSsl = true;//经过ssl加密 
            object userState = msg; 
            client.SendAsync(msg, userState); 
        }
    }
}