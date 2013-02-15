using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ContestHunter.Models.Domain;
using System.Web;
using System.Collections.Generic;
using System.Threading;
using ContestHunter.Database;

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
            User info = User.ByName("name");
            Assert.AreEqual("name", info.Name);
            Assert.AreEqual("variantf@gmail.com", info.Email);
            Assert.AreEqual("Administrator", User.CurrentUserName);
            var groups = Group.All();
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(new Group() { Name = "Administrators" }, groups[0]);
            groups = User.ByName("Administrator").Groups();
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual("Administrators", groups[0].Name);
            Group grp=Group.ByName("Administrators");
            Assert.AreEqual(1, grp.UserCount());
            Group.Add(new Group() { Name = "blabla" });
            Group.ByName("blabla").AddUser(User.ByName("name"));
            Assert.AreEqual("blabla", User.ByName("name").Groups()[0].Name);
            Group.ByName("blabla").RemoveUser(User.ByName("name"));
            Assert.AreEqual(0, Group.ByName("blabla").Users(0,10).Count);
            Group.ByName("blabla").Remove();
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
                Name = "Test 12",
                Description = "This is Description",
                RelativeStartTime = DateTime.Now + Start,
                RelativeEndTime = DateTime.Now + Duration,
                IsOfficial = false,
                Owner = new List<string>() { "123" },
                Type = Contest.ContestType.OI
            });
            var nowtest = Contest.ByName("Test 12");
            Assert.AreEqual(0,nowtest.AttendedUsersCount());
            nowtest.Attend();
            Assert.AreEqual(true, nowtest.IsAttended());
            Assert.AreEqual(1, nowtest.AttendedUsersCount());
            Assert.AreEqual("123", nowtest.AttendedUsers(0, 10)[0]);
            nowtest.Disattended();
            Assert.AreEqual(0, nowtest.AttendedUsersCount());
            Assert.AreEqual(false, nowtest.IsAttended());

            nowtest.AddProblem(new Problem()
            {
                Comparer = "blabla",
                Content = "pilapila",
                Name = "Name"
            });
            nowtest.AddProblem(new Problem()
            {
                Comparer = "blabla",
                Content = "pilapila",
                Name = "Name2"
            });
            Assert.AreEqual(2, nowtest.Problems().Count);
            nowtest.RemoveProblem("Name2");
            Assert.AreEqual(1, nowtest.Problems().Count);
            nowtest.Attend();
            nowtest.ProblemByName("Name").Submit(new Record() { Code = "This is the code.", Language = Record.LanguageType.CPP });
            User.Authenticate(User.Login("Mr.Phone", "chshcan"));
            nowtest.Attend();
            Thread.Sleep(60000);
            Assert.AreEqual("pilapila",nowtest.ProblemByName("Name").Content);

            nowtest.ProblemByName("Name").Submit(new Record() { Code = "This is the code.", Language = Record.LanguageType.CPP });
        }

        [TestMethod]
        public void TestTestCase()
        {
            User.Authenticate(User.Login("123", "123"));
            var prob = Contest.ByName("Test 10").ProblemByName("Name");
            var tid=prob.AddTestCase(new TestCase()
            {
                Input = new byte[] { 1, 2, 1, 3, 8 },
                Data = new byte[] { 1, 2, 1, 3, 8 },
                TimeLimit = 1000,
                MemoryLimit = 1000
            });
            Assert.AreEqual(1, prob.TestCases().Count);
            Assert.AreEqual(tid, prob.TestCases()[0]);
            Assert.AreEqual(1000, prob.TestCaseByID(tid).TimeLimit);
            prob.RemoveTestCase(tid);
            Assert.AreEqual(0, prob.TestCases().Count);
        }

        [TestMethod]
        public void TestStanding()
        {
            var con = Contest.ByName("Test 12");
            var stand = con.GetACMStanding(0, 10);
            Assert.AreEqual(0, (int)Record.StatusType.Accept);
        }

        [TestMethod]
        public void TestVirtual()
        {
            User.Authenticate(User.Login("123", "123"));
            var con = Contest.ByName("Test 12");
            var tm = DateTime.Now;
            con.VirtualAttend(tm);
            con.ProblemByName("Name").Submit(new Record()
            {
                Code = "virtual test",
                Language = Record.LanguageType.Pascal
            });
            Assert.AreEqual(tm, con.RelativeStartTime);
        }
        [TestMethod]
        public void TestDB()
        {

        }
    }
}

