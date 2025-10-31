using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SqlClient;
using System.IO;
using BookStoreLIB;

namespace BookStoreLIB
{
    [TestClass]
    public class LoginUnitTests
    {
        private UserData userData;
        public TestContext TestContext { get; set; }

        private const string DefaultServer = "tfs.cs.uwindsor.ca";
        private const string DefaultDb = "Agile1422DB25";
        private const string LocalConnTemplate =
            "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\BookStoreDB.mdf;Integrated Security=True";

        [TestInitialize]
        public void Setup()
        {
            TryLoadDotEnv();

            var dataDir = TryFindDatabaseFolder();
            if (dataDir != null)
            {
                AppDomain.CurrentDomain.SetData("DataDirectory", dataDir);
                TestContext?.WriteLine("|DataDirectory| => " + dataDir);
            }

            var remote = BuildRemoteFromEnv();
            var local = LocalConnTemplate;

            string chosen;
            Exception remoteErr;
            if (!string.IsNullOrWhiteSpace(remote) && CanOpen(remote, 3, out remoteErr))
            {
                chosen = remote;
                TestContext?.WriteLine("DB: Using REMOTE (env-based).");
            }
            else
            {
                chosen = local;
                TestContext?.WriteLine("DB: Using LOCAL MDF.");
            }

            BookStoreLIB.Properties.Settings.Default["dbConnectionString"] = chosen;
            PrintConnectionSummary(chosen);

            userData = new UserData();
        }

        [TestMethod]
        public void ValidLogin_ShouldReturnTrue()
        {
            // Arrange
            var dal = new DALUserInfo();
            var username = "test_" + Guid.NewGuid().ToString("N").Substring(0, 6);
            var password = "Password123";
            var email = username + "@example.com";
            var fullname = "Temporary Tester";

            // Register new user (which hashes the password automatically)
            var created = dal.RegisterUser(fullname, username, password, email);
            Assert.IsTrue(created, "User registration failed — maybe username already exists?");

            // Act
            var userData = new UserData();
            bool ok = userData.LogIn(username, password);

            // Assert
            Assert.IsTrue(ok, "Expected login to succeed with correct password.");
            Assert.AreNotEqual(0, userData.UserID, "UserID should be populated after login.");
        }


        [TestMethod]
        public void InvalidUsername_ShouldReturnFalse()
        {
            bool ok = userData.LogIn("notexist", "xx1234");
            Assert.IsFalse(ok);
        }

        [TestMethod]
        public void PasswordTooShort_ShouldThrowArgumentException()
        {
            var ex = Assert.ThrowsException<ArgumentException>(() => userData.LogIn("dclark", "dc12"));
            StringAssert.Contains(ex.Message, "at least six characters");
        }

        [TestMethod]
        public void PasswordStartsWithDigit_ShouldThrowArgumentException()
        {
            var ex = Assert.ThrowsException<ArgumentException>(() => userData.LogIn("dclark", "1c1234"));
            StringAssert.Contains(ex.Message, "start with a letter");
        }

        [TestMethod]
        public void ManagerLogin_SetsIsManagerTrue()
        {
            // Arrange
            var dal = new DALUserInfo();
            var username = "test_" + Guid.NewGuid().ToString("N").Substring(0, 6);
            var password = "Password123";
            var email = username + "@example.com";
            var fullname = "Temporary Tester";

            // Step 1: Register normally (will insert Manager = 0, Type = CU by default)
            bool created = dal.RegisterUser(fullname, username, password, email);
            Assert.IsTrue(created, "Failed to register test user.");

            // Step 2: Promote this test user to Manager in the database
            using (var conn = new SqlConnection(
                "Data Source=tfs.cs.uwindsor.ca;Initial Catalog=Agile1422DB25;Persist Security Info=True;User ID=Agile1422U25;Password=Agile1422U25$;Encrypt=True;TrustServerCertificate=True"))
            {
                conn.Open();
                var promoteCmd = new SqlCommand(
                    "UPDATE dbo.UserData SET Manager = 1, [Type] = 'AD' WHERE UserName = @UserName", conn);
                promoteCmd.Parameters.AddWithValue("@UserName", username);
                promoteCmd.ExecuteNonQuery();
            }

            // Step 3: Act — log in using the business layer
            var ud = new UserData();
            bool loggedIn = ud.LogIn(username, password);

            // Assert
            Assert.IsTrue(loggedIn, "Manager login failed.");
            Assert.IsTrue(ud.IsManager, "Manager flag should be true.");
            Assert.AreEqual("AD", ud.Type, "Manager Type should be 'AD'.");
        }

        [TestMethod]
        public void NonManagerLogin_SetsIsManagerFalse()
        {
            // Arrange
            var dal = new DALUserInfo();
            var username = "test_" + Guid.NewGuid().ToString("N").Substring(0, 6);
            var password = "Password123";
            var email = username + "@example.com";
            var fullname = "Temporary Customer";

            // Step 1: Register the user (default Manager=0, Type='CU')
            bool created = dal.RegisterUser(fullname, username, password, email);
            Assert.IsTrue(created, "Failed to register test customer.");

            // Step 2: Act — log in using business layer
            var ud = new UserData();
            bool loggedIn = ud.LogIn(username, password);

            // Step 3: Assert
            Assert.IsTrue(loggedIn, "Customer login failed.");
            Assert.IsFalse(ud.IsManager, "Customer should not be manager.");
            Assert.AreEqual("CU", ud.Type, "Customer Type should be 'CU'.");
        }

        //remote connection builder with ENV
        private static string BuildRemoteFromEnv()
        {
            var user = Environment.GetEnvironmentVariable("AGILE_DB_USER");
            var pass = Environment.GetEnvironmentVariable("AGILE_DB_PASSWORD");
            var server = Environment.GetEnvironmentVariable("AGILE_DB_SERVER") ?? DefaultServer;
            var db = Environment.GetEnvironmentVariable("AGILE_DB_NAME") ?? DefaultDb;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass)) return null;
            var sb = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = db,
                PersistSecurityInfo = true,
                UserID = user,
                Password = pass,
                Encrypt = true,
                TrustServerCertificate = true
            };
            return sb.ConnectionString;
        }

        // check if we can open
        private static bool CanOpen(string connStr, int timeoutSeconds, out Exception err)
        {
            try
            {
                var sb = new SqlConnectionStringBuilder(connStr) { ConnectTimeout = timeoutSeconds };
                using (var conn = new SqlConnection(sb.ConnectionString))
                {
                    conn.Open();
                    err = null;
                    return true;
                }
            }
            catch (Exception ex)
            {
                err = ex;
                return false;
            }
        }

        // trouble shooting
        private void PrintConnectionSummary(string connStr)
        {
            try
            {
                var normalized = connStr.Replace("Trust Server Certificate", "TrustServerCertificate");
                var sb = new SqlConnectionStringBuilder(normalized);
                if (sb.ContainsKey("User ID")) sb.UserID = "***";
                if (sb.ContainsKey("Password")) sb.Password = "***";
                TestContext?.WriteLine(
                    "DB Summary -> DataSource: " + sb.DataSource +
                    ", InitialCatalog: " + sb.InitialCatalog +
                    ", AttachDbFilename: " + sb.AttachDBFilename +
                    ", Encrypt: " + sb.Encrypt +
                    ", TrustServerCertificate: " + sb.TrustServerCertificate
                );
            }
            catch (Exception ex)
            {
                var safe = connStr.Replace("Password=", "Password=***").Replace("User ID=", "User ID=***");
                TestContext?.WriteLine("DB Summary -> (raw, masked): " + safe + " | parse error: " + ex.Message);
            }
        }

        //load env from wherever we store it
        private static void TryLoadDotEnv()
        {
            try
            {
                var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                for (int i = 0; i < 6 && dir != null; i++, dir = dir.Parent)
                {
                    var envPath = Path.Combine(dir.FullName, ".env");
                    if (!File.Exists(envPath)) continue;
                    foreach (var raw in File.ReadAllLines(envPath))
                    {
                        var line = raw.Trim();
                        if (line.Length == 0 || line.StartsWith("#")) continue;
                        var idx = line.IndexOf('=');
                        if (idx <= 0) continue;
                        var key = line.Substring(0, idx).Trim();
                        var val = line.Substring(idx + 1).Trim().Trim('"');
                        Environment.SetEnvironmentVariable(key, val, EnvironmentVariableTarget.Process);
                    }
                    break;
                }
            }
            catch { }
        }

        //fallback db location checking
        private static string TryFindDatabaseFolder()
        {
            var cur = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            for (int i = 0; i < 8 && cur != null; i++, cur = cur.Parent)
            {
                var candidate = Path.Combine(cur.FullName, "BookStoreGUI", "Database");
                var mdf = Path.Combine(candidate, "BookStoreDB.mdf");
                if (File.Exists(mdf)) return candidate;
            }
            return null;
        }
    }
}
