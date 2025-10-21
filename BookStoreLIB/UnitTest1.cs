using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SqlClient;
using System.IO;
using BookStoreLIB;
using DotNetEnv;


namespace BookStoreLIB
{
    [TestClass]
    public class UnitTest1
    {
        private UserData userData;
        public TestContext TestContext { get; set; }

        // Keep your originals as defaults
        private const string RemoteConnDefault =
            "Data Source=tfs.cs.uwindsor.ca;Initial Catalog=Agile1422DB25;Persist Security Info=True;User ID=Agile1422U25;Password=Agile1422U25$;Encrypt=True;TrustServerCertificate=True";

        private const string LocalConnTemplate =
            "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\BookStoreDB.mdf;Integrated Security=True";

        [TestInitialize]
        public void Setup()
        {
            // Try to load .env (optional)
            TryLoadDotEnv();

            // |DataDirectory| for local MDF
            var dataDir = TryFindDatabaseFolder();
            if (dataDir != null)
            {
                AppDomain.CurrentDomain.SetData("DataDirectory", dataDir);
                TestContext?.WriteLine("|DataDirectory| => " + dataDir);
            }
            else
            {
                TestContext?.WriteLine("WARNING: Could not locate BookStoreGUI\\Database folder from test bin directory.");
            }

            // Build remote string: prefer env vars if present, else your default
            var remote = BuildRemoteConnFromEnv() ?? RemoteConnDefault;
            var local = LocalConnTemplate;

            // Prefer remote only if it opens quickly AND has dbo.UserData
            string chosen;
            Exception remoteErr;
            if (CanOpen(remote, 3, out remoteErr) && TableExists(remote, "dbo", "UserData"))
            {
                chosen = remote;
                TestContext?.WriteLine("DB: Using REMOTE connection (BookStoreRemote).");
            }
            else
            {
                if (remoteErr != null)
                    TestContext?.WriteLine("DB: Remote unavailable. Error: " + remoteErr.Message);
                else
                    TestContext?.WriteLine("DB: Remote opened but dbo.UserData missing; using LOCAL MDF.");

                chosen = local;
                TestContext?.WriteLine("DB: Using LOCAL MDF connection (dbConnectionString).");
            }

            // Override the library's connection string for the test run
            BookStoreLIB.Properties.Settings.Default["dbConnectionString"] = chosen;

            // Print masked connection summary
            PrintConnectionSummary(chosen);

            userData = new UserData();
        }

        // -------------------
        //        TESTS
        // -------------------

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
                delegate { userData.LogIn("dclark", "dc12"); });
            StringAssert.Contains(ex.Message, "at least six characters");
        }

        [TestMethod]
        public void PasswordStartsWithDigit_ShouldThrowArgumentException()
        {
            var ex = Assert.ThrowsException<ArgumentException>(
                delegate { userData.LogIn("dclark", "1c1234"); });
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

        // -------------------
        //     HELPERS
        // -------------------

        private static void TryLoadDotEnv()
        {
            try
            {
                // Works if DotNetEnv is installed; otherwise no-op
                var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                for (int i = 0; i < 6 && dir != null; i++, dir = dir.Parent)
                {
                    var envPath = Path.Combine(dir.FullName, ".env");
                    if (File.Exists(envPath))
                    {
                        // DotNetEnv optional dependency
                        try { DotNetEnv.Env.Load(envPath); } catch { /* ignore */ }
                        break;
                    }
                }
            }
            catch { /* ignore */ }
        }

        private static string BuildRemoteConnFromEnv()
        {
            var user = Environment.GetEnvironmentVariable("AGILE_DB_USER");
            var pass = Environment.GetEnvironmentVariable("AGILE_DB_PASSWORD");
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
                return null;

            // Build from env (same server/catalog as your default)
            var sb = new SqlConnectionStringBuilder
            {
                DataSource = "tfs.cs.uwindsor.ca",
                InitialCatalog = "Agile1422DB25",
                PersistSecurityInfo = true,
                UserID = user,
                Password = pass,
                Encrypt = true,
                TrustServerCertificate = true
            };
            return sb.ConnectionString;
        }

        private static bool CanOpen(string connStr, int timeoutSeconds, out Exception err)
        {
            SqlConnection conn = null;
            try
            {
                var withTimeout = connStr;
                if (IndexOfIgnoreCase(connStr, "connect timeout=") < 0)
                    withTimeout = connStr.TrimEnd(';') + ";Connect Timeout=" + timeoutSeconds;

                conn = new SqlConnection(withTimeout);
                conn.Open();
                err = null;
                return true;
            }
            catch (Exception ex)
            {
                err = ex;
                return false;
            }
            finally
            {
                if (conn != null) conn.Dispose();
            }
        }

        private static bool TableExists(string connStr, string schema, string table)
        {
            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand(
                "SELECT 1 FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id " +
                "WHERE t.name=@t AND s.name=@s", conn))
            {
                cmd.Parameters.AddWithValue("@t", table);
                cmd.Parameters.AddWithValue("@s", schema);
                conn.Open();
                var r = cmd.ExecuteScalar();
                return r != null;
            }
        }

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
                var safe = connStr
                    .Replace("Password=", "Password=***")
                    .Replace("User ID=", "User ID=***");
                TestContext?.WriteLine("DB Summary -> (raw, masked): " + safe + " | parse error: " + ex.Message);
            }
        }

        private static int IndexOfIgnoreCase(string haystack, string needle)
        {
            return haystack != null
                ? haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase)
                : -1;
        }

        private static string TryFindDatabaseFolder()
        {
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
    }
}
