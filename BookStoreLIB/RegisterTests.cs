using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using BookStoreLIB;

namespace BookStoreLIB
{
    [TestClass]
    public class RegisterTests
    {
        [ClassInitialize]
        public static void LoadEnv(TestContext _)
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            for (int i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
            {
                var p = Path.Combine(dir.FullName, ".env");
                if (!File.Exists(p)) continue;
                foreach (var raw in File.ReadAllLines(p))
                {
                    var line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith("#")) continue;
                    int idx = line.IndexOf('=');
                    if (idx <= 0) continue;
                    var k = line.Substring(0, idx).Trim();
                    var v = line.Substring(idx + 1).Trim().Trim('"');
                    Environment.SetEnvironmentVariable(k, v, EnvironmentVariableTarget.Process);
                }
                break;
            }
        }

        private static string U(string p = "u")
        {
            return (p + "_" + Guid.NewGuid().ToString("N")).Substring(0, 16);
        }

        [TestMethod]
        public void RegisterUniqueUser_ThenLoginSucceeds()
        {
            var dal = new DALUserInfo();
            var user = U("reg");
            var email = user + "@example.com";
            Assert.IsTrue(dal.RegisterUser("Test User", user, "Abc123", email));

            var ud = new UserData();
            Assert.IsTrue(ud.LogIn(user, "Abc123"));
            Assert.AreEqual("CU", ud.Type);
            Assert.IsFalse(ud.IsManager);
        }

        [TestMethod]
        public void RegisterDuplicateUsername_ReturnsFalse()
        {
            var dal = new DALUserInfo();
            var user = U("dup");
            var email = user + "@example.com";
            Assert.IsTrue(dal.RegisterUser("Test User", user, "Abc123", email));
            Assert.IsFalse(dal.RegisterUser("Test User", user, "Abc123", email));
        }
    }
}
