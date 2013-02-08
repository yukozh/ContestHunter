using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ContestHunter.Models.Domain;
using System.Web;

namespace DomainTest
{
    [TestClass]
    public class DomainUser
    {
        [TestMethod]
        public void TestSendEmail()
        {
            
//            User.SendValidationEmail("name", "password", "variantf@gmail.com", "http://contesthunter.com/regiser");
//            User.Register("name", "password", HttpUtility.UrlDecode("LH4FVB8uwbL%2bk1QGPIYAEA%3d%3d"), "variantf@gmail.com", HttpUtility.UrlDecode("Cw%2fiKgKrOnHQ4QiPeyydziybbu1%2f3Z2b"));
        }
        [TestMethod]
        public void TestLoginout()
        {
            string token=User.Login("123", "123");
            string[] guids = token.Split(new char[] { '|' });
            Assert.AreEqual(2, guids.Length);
            Guid guid1 = Guid.Parse(guids[0]), guid2 = Guid.Parse(guids[1]);
            User.Authenticate(token);
            User info = User.SelectByName("name");
            Assert.AreEqual("name", info.name);
            Assert.AreEqual("variantf@gmail.com", info.email);
            User.Logout();
        }
    }
}

