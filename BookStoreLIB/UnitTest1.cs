using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SqlClient;
using System.IO;
using BookStoreLIB;

namespace BookStoreLIB
{
    [TestClass]
    public class UnitTest1
    {
        private UserData userData;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Setup()
        {
            // Dynamically find DB, fixed our errors for test cases
            // Writes out where it found the DB to confirm
            var dataDir = TryFindDatabaseFolder();
            if (dataDir != null)
            {
                AppDomain.CurrentDomain.SetData("DataDirectory", dataDir);
                TestContext?.WriteLine($"|DataDirectory| => {dataDir}");
            }
            else
            {
                // Tests might fail if this renders
                TestContext?.WriteLine("WARNING: Could not locate BookStoreGUI\\Database folder from test bin directory.");
            }

            userData = new UserData();
        }

        private static string TryFindDatabaseFolder()
        {
            // We look for DB 8 paths deep
            var cur = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            for (int i = 0; i < 8 && cur != null; i++, cur = cur.Parent)
            {
                var candidate = Path.Combine(cur.FullName, "BookStoreGUI", "Database");
                var mdf = Path.Combine(candidate, "BookStoreDB.mdf");
                if (File.Exists(mdf))
                    return candidate;
            }
            return null;
        }

        [TestMethod]
        public void ValidLogin_ShouldReturnTrue()
        {
            bool ok = userData.LogIn("dclark", "dc1234");
            int userId = userData.UserID;

            Assert.IsTrue(ok, "Expected valid login to return true.");
            Assert.AreEqual(1, userId, "Expected UserID=1 for dclark.");
        }

        [TestMethod]
        public void InvalidUsername_ShouldReturnFalse()
        {
            bool ok = userData.LogIn("notexist", "xx1234"); 
            Assert.IsFalse(ok, "Unknown username should return false.");
        }

        [TestMethod]
        public void PasswordTooShort_ShouldThrowArgumentException()
        {
            var ex = Assert.ThrowsException<ArgumentException>(
                () => userData.LogIn("dclark", "dc12")); 
            StringAssert.Contains(ex.Message, "at least six characters");
        }

        [TestMethod]
        public void PasswordStartsWithDigit_ShouldThrowArgumentException()
        {
            var ex = Assert.ThrowsException<ArgumentException>(
                () => userData.LogIn("dclark", "1c1234")); 
            StringAssert.Contains(ex.Message, "start with a letter");
        }

        [TestMethod]
        public void ManagerLogin_SetsIsManagerTrue()
        {
            var ud = new UserData();
            Assert.IsTrue(ud.LogIn("mjones", "mj1234"));
            Assert.IsTrue(ud.IsManager);
        }

        [TestMethod]
        public void NonManagerLogin_SetsIsManagerFalse()
        {
            var ud = new UserData();
            Assert.IsTrue(ud.LogIn("dclark", "dc1234"));
            Assert.IsFalse(ud.IsManager);
        }

    }

}
