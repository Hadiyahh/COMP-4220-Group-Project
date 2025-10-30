#if DEBUG
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace BookStoreLIB.Tests
{
    [TestClass]
    public class AdminLoginTests
    {
        [TestMethod]
        public void Admin_Login_Succeeds_And_Type_Is_AD()
        {
            var u = new UserData();
            Assert.IsTrue(u.LogIn("admin", "admin123"));
            Assert.AreEqual("AD", u.Type);
        }

        [TestMethod]
        public void Customer_Login_Succeeds_And_Type_Is_CU()
        {
            var u = new UserData();
            Assert.IsTrue(u.LogIn("sanghvi2", "Aryan123"));
            Assert.AreEqual("CU", u.Type);
        }

        [TestMethod]
        public void Invalid_Login_Fails()
        {
            var u = new UserData();
            try
            {
                // Use a compliant but wrong password to avoid validation throw
                bool ok = u.LogIn("nope", "Wrong123");
                Assert.IsFalse(ok, "Unknown user should fail login.");
            }
            catch (ArgumentException)
            {
                // Also acceptable if LogIn enforces policy via exceptions
                Assert.IsTrue(true);
            }
        }

    }
}
#endif
