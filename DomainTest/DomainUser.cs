using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ContestHunter.Models.Domain;
using System.Web;
using System.Collections.Generic;

namespace DomainTest
{
    [TestClass]
    public class DomainUser
    {
        [TestMethod]
        public void TestSendEmail()
        {
            
//            User.SendValidationEmail("name", "password", "variantf@gmail.com", "http://contesthunter.com/regiser");
            User.Register("name", "password", HttpUtility.UrlDecode("LH4FVB8uwbL%2bk1QGPIYAEA%3d%3d"), "variantf@gmail.com", HttpUtility.UrlDecode("Cw%2fiKgKrOnHQ4QiPeyydziybbu1%2f3Z2b"));
        }
        [TestMethod]
        public void TestUser()
        {
            string token=User.Login("Administrator", "07070078899");
            string[] guids = token.Split(new char[] { '|' });
            Assert.AreEqual(2, guids.Length);
            Guid guid1 = Guid.Parse(guids[0]), guid2 = Guid.Parse(guids[1]);
            User.Authenticate(token);
            User info = User.SelectByName("name");
            Assert.AreEqual("name", info.Name);
            Assert.AreEqual("variantf@gmail.com", info.Email);
            Assert.AreEqual("Administrator", User.CurrentUserName);
            var groups = Group.All();
            Assert.AreEqual(1, groups.Length);
            Assert.AreEqual("Administrators", groups[0]);
            groups = Group.ByUsername("Administrator");
            Assert.AreEqual(1, groups.Length);
            Assert.AreEqual("Administrators", groups[0]);
            Assert.AreEqual(1, Group.Users("Administrators").Length);
            Group.Add("blabla");
            Group.AddUser("blabla", "name");
            Assert.AreEqual("blabla", Group.ByUsername("name")[0]);
            Group.RemoveUser("blabla", "name");
            Assert.AreEqual(0, Group.ByUsername("name").Length);
            Group.Remove("blabla");
            User.Logout();
        }

        [TestMethod]
        public void TestFramework()
        {
            Framework.DomainInstallation();
        }

        [TestMethod]
        public void TestContest()
        {
            User.Authenticate(User.Login("123", "123"));
            Assert.AreEqual(0, Contest.Pending(0, 10).Count);
            Assert.AreEqual(2, Contest.Testing(0, 10).Count);
            Assert.AreEqual(0, Contest.Done(0, 10).Count);
        }
    }
}

