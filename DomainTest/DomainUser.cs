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
            User.SendValidationEmail("abc", "abc", "VariantF@gmail.com");
        }
    }
}
