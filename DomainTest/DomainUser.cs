using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ContestHunter.Models.Domain;

namespace DomainTest
{
    [TestClass]
    public class DomainUser
    {
        [TestMethod]
        public void TestSendEmail()
        {
            User.SendValidationEmail("name", "password", "variantf@gmail.com", "http://contesthunter.com/regiser");
//            User.Register("123", "123", ".%a5%16%85%f3%810%93", "123");
        }
        [TestMethod]
        public void TestLoginout()
        {
            string token=User.Login("123", "123");
            User.Authenticate(token);
            User.Logout();
        }
    }
}

