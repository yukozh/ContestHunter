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
            var Start = TimeSpan.FromMinutes(1.0);
            var Duration = TimeSpan.FromHours(1.0);
            Contest.Add(new Contest()
            {
                Name = "Test 1",
                Description = "This is Description",
                StartTime = DateTime.Now + Start,
                EndTime = DateTime.Now + Duration,
                IsOfficial = false,
                Owner = new List<string>() { "123" },
                Type = Contest.ContestType.OI
            });
            var nowtest = Contest.ByName("Test 1");
            Assert.AreEqual(0,nowtest.AttendedUsersCount());
            nowtest.Attend();
            Assert.AreEqual(true, nowtest.IsAttended());
            Assert.AreEqual(1, nowtest.AttendedUsersCount());
            Assert.AreEqual("123", nowtest.AttendedUsers(0, 10)[0]);
            nowtest.Disattended();
            Assert.AreEqual(0, nowtest.AttendedUsersCount());
            Assert.AreEqual(false, nowtest.IsAttended());

        }
    }
}

