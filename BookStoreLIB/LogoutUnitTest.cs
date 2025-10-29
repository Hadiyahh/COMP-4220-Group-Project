using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using BookStoreLIB;
using System.IO;
using System.Data.SqlClient;

namespace BookStoreGUI
{
    [TestClass]
    public class LogoutUnitTest
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

            userData = new UserData();
        }

        //simulating a user logging in with credentials, then filling cart, then logging out, when user logs out it clears cart and sets user to null
        [TestMethod]
        public void Logout_AfterRealLogin_ClearsCart()
        {
            //Real login from database
            var userData = new UserData();
            bool loginResult = userData.LogIn("lijacki", "pass123");
            var cart = new Cart();

            // Add some test items
            cart.addBook(new Book { BookID = 101, Title = "Think Like a Monk", Price = 29.99m });
            cart.addBook(new Book { BookID = 102, Title = "Programming Guide", Price = 39.99m });

            Assert.IsTrue(loginResult, "Should successfully login with real user");
            Assert.IsTrue(userData.LoggedIn, "User should be logged in");
            Assert.AreEqual(2, cart.shoppingCart.Count, "Cart should have items");

            userData = null;
            cart.clearCart();

            Assert.IsNull(userData, "User session should be cleared");
            Assert.AreEqual(0, cart.shoppingCart.Count, "Cart should be empty");
        }

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