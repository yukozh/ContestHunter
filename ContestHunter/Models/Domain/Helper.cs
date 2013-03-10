using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.IO;
using System.Net.Mail;

namespace ContestHunter.Models.Domain
{
    static class Helper
    {
        public static string GetLegalName(string name)
        {
            return name.Replace('+', '＋').Replace('.', '．').Replace('/', '／').Replace('#', '＃');
        }
    }

    static class DESHelper
    {
        static byte[] IV;
        static byte[] Key;

        static DESHelper()
        {
            using (DES des = DESCryptoServiceProvider.Create())
            {
                des.GenerateIV();
                des.GenerateKey();
                IV = des.IV;
                Key = des.Key;
            }
        }


        public static byte[] Encrypt(byte[] input)
        {
            using (DES des = DESCryptoServiceProvider.Create())
            {
                using (ICryptoTransform encryptor = des.CreateEncryptor(Key, IV))
                {
                    using (MemoryStream mem = new MemoryStream())
                    {
                        using (CryptoStream stream = new CryptoStream(mem, encryptor, CryptoStreamMode.Write))
                        {
                            stream.Write(input, 0, input.Length);
                        }
                        return mem.ToArray();
                    }
                }
            }
        }

        public static byte[] Decrypt(byte[] input)
        {
            using (DES des = DESCryptoServiceProvider.Create())
            {
                using (ICryptoTransform decryptor = des.CreateDecryptor(Key, IV))
                {
                    using (MemoryStream mem = new MemoryStream())
                    {
                        using (CryptoStream stream = new CryptoStream(mem, decryptor, CryptoStreamMode.Write))
                        {
                            stream.Write(input, 0, input.Length);
                        }
                        return mem.ToArray();
                    }
                }

            }
        }
    }

    static class EmailHelper
    {
        public static void Send(string Subject,string To, string Content)
        {
            using (MailMessage msg = new MailMessage())
            {
                msg.From = new MailAddress("contesthunter@163.com");
                msg.To.Add(To);
                msg.Subject = Subject;
                msg.Body = Content;
                msg.IsBodyHtml = true;

                SmtpClient client = new SmtpClient();
                client.Credentials = new System.Net.NetworkCredential("contesthunter@163.com", "AppleStore123");
                client.Port = 25;
                client.Host = "smtp.163.com";
                //client.EnableSsl = true;
                client.Send(msg);
            }
        }
    }

    static class LogHelper
    {
        public static void WriteLog(string Title,string Content)
        {
            try
            {
                string logItem = string.Format("LOG:{0} {1}:\r\n{2}\r\n", DateTime.Now, Title, Content);
                File.AppendAllText(Path.Combine(Framework.WebRoot, "App_Data\\Daemon.log"), logItem);
            }
            catch { }
        }
    }
}