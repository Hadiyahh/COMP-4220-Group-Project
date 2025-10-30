using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace BookStoreLIB
{
    /// <summary>
    /// Integration tests for Registration.
    /// Safe for the team: creates unique throwaway users and cleans them up.
    /// Requires BookStoreLIB.app.config to have "BookStoreRemote" connection string,
    /// and %AGILE_DB_*% env vars to be present (as in your project).
    /// </summary>
    [TestClass]
    public class RegisterTests
    {
        private static string _conn; // resolved connection string
        private static readonly System.Collections.Generic.List<string> _created =
            new System.Collections.Generic.List<string>();

        [ClassInitialize]
        public static void ClassInit(TestContext _)
        {
            _conn = ResolveConn();
            // quick connectivity probe so failures are obvious
            using (var c = new SqlConnection(_conn)) { c.Open(); }
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // best-effort cleanup of any users we created
            foreach (var u in _created)
                SafeDeleteUser(u);
        }

        // ------------ TESTS ------------

        [TestMethod]
        [TestCategory("Integration")]
        public void Register_NewUser_Succeeds_ThenCanLogin()
        {
            // arrange
            string uname = "ut_reg_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            string fname = "Unit Test User";
            string email = uname + "@example.com";
            string pass = "Passw0rd!";

            var dal = new DALUserInfo();

            try
            {
                // act: register
                bool ok = dal.RegisterUser(fname, uname, pass, email);

                // assert
                Assert.IsTrue(ok, "Registration should succeed for a new unique username.");

                _created.Add(uname); // track for cleanup

                // act: login
                int userId = dal.LogIn(uname, pass);
                Assert.IsTrue(userId > 0, "Login should succeed after registration.");

                // act: fetch row to verify stored fields
                var row = GetUserRow(uname);
                Assert.IsNotNull(row, "Inserted user row must exist.");

                // Email persisted
                Assert.AreEqual(email, row.Email, "Email should be saved for new user.");

                // Manager + Type defaults (course convention)
                Assert.AreEqual(false, row.Manager, "New customer should not be a manager.");
                Assert.AreEqual("CU", row.Type, "New user Type should be 'CU'.");

                // Password is hashed (no plaintext)
                Assert.AreEqual("***", row.PasswordMasked, "Plaintext password must not be stored.");
                Assert.IsTrue(row.HashLen >= 16 && row.SaltLen >= 8,
                    "PasswordHash/PasswordSalt should be set for new user.");
            }
            finally
            {
                // optional per-test cleanup (kept for safety; ClassCleanup also removes leftovers)
                SafeDeleteUser(uname);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Register_DuplicateUsername_Fails()
        {
            string uname = "ut_reg_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            string fname = "Unit Test User";
            string email = uname + "@example.com";
            string pass = "Passw0rd!";

            var dal = new DALUserInfo();

            try
            {
                // first insert
                bool ok1 = dal.RegisterUser(fname, uname, pass, email);
                _created.Add(uname);
                Assert.IsTrue(ok1, "First registration should succeed.");

                // second insert with same username
                bool ok2 = dal.RegisterUser(fname, uname, pass, email);
                Assert.IsFalse(ok2, "RegisterUser must return false when username already exists.");
            }
            finally
            {
                SafeDeleteUser(uname);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Register_PasswordStoredHashed_NotPlaintext()
        {
            string uname = "ut_reg_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            string fname = "Unit Test User";
            string email = uname + "@example.com";
            string pass = "Passw0rd!";

            var dal = new DALUserInfo();

            try
            {
                bool ok = dal.RegisterUser(fname, uname, pass, email);
                _created.Add(uname);
                Assert.IsTrue(ok, "Registration should succeed.");

                var row = GetUserRow(uname);
                Assert.IsNotNull(row, "Row must exist.");

                // Verify hashing columns
                Assert.AreEqual("***", row.PasswordMasked, "Legacy [Password] column should be masked as ***.");
                Assert.IsTrue(row.HashLen >= 16, "PasswordHash should be populated.");
                Assert.IsTrue(row.SaltLen >= 8, "PasswordSalt should be populated.");
            }
            finally
            {
                SafeDeleteUser(uname);
            }
        }

        // ------------ Helpers ------------

        private static string ResolveConn()
        {
            // Prefer config connection string (with %AGILE_*% placeholders)
            var raw = ConfigurationManager.ConnectionStrings["BookStoreRemote"]?.ConnectionString;
            if (!string.IsNullOrWhiteSpace(raw))
            {
                var expanded = Environment.ExpandEnvironmentVariables(raw);
                var b = new SqlConnectionStringBuilder(expanded);
                return b.ConnectionString;
            }

            // Fallback to environment variables directly
            var user = Environment.GetEnvironmentVariable("AGILE_DB_USER");
            var pass = Environment.GetEnvironmentVariable("AGILE_DB_PASSWORD");
            var server = Environment.GetEnvironmentVariable("AGILE_DB_SERVER") ?? "tfs.cs.uwindsor.ca";
            var db = Environment.GetEnvironmentVariable("AGILE_DB_NAME") ?? "Agile1422DB25";

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
                throw new InvalidOperationException("Missing AGILE_DB_USER/AGILE_DB_PASSWORD.");

            var cs = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = db,
                PersistSecurityInfo = true,
                UserID = user,
                Password = pass,
                Encrypt = true,
                TrustServerCertificate = true
            };
            return cs.ConnectionString;
        }

        private static void SafeDeleteUser(string username)
        {
            if (string.IsNullOrEmpty(username)) return;
            try
            {
                using (var c = new SqlConnection(_conn))
                using (var cmd = new SqlCommand("DELETE FROM dbo.UserData WHERE UserName=@U", c))
                {
                    cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = username;
                    c.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // swallow: test cleanup should never fail the run
            }
        }

        private struct UserRow
        {
            public string Email;
            public bool Manager;
            public string Type;
            public string PasswordMasked;
            public int HashLen;
            public int SaltLen;
        }

        private static UserRow GetUserRow(string username)
        {
            using (var c = new SqlConnection(_conn))
            using (var cmd = new SqlCommand(
                @"SELECT Email, Manager, [Type], [Password],
                         DATALENGTH(PasswordHash) AS HashLen,
                         DATALENGTH(PasswordSalt) AS SaltLen
                  FROM dbo.UserData WHERE UserName=@U", c))
            {
                cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = username;
                c.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) throw new AssertFailedException("User row not found.");
                    var row = new UserRow();
                    row.Email = r.IsDBNull(0) ? null : r.GetString(0);
                    row.Manager = !r.IsDBNull(1) && r.GetBoolean(1);
                    row.Type = r.IsDBNull(2) ? null : r.GetString(2);
                    row.PasswordMasked = r.IsDBNull(3) ? null : r.GetString(3);
                    row.HashLen = r.IsDBNull(4) ? 0 : Convert.ToInt32(r.GetValue(4));
                    row.SaltLen = r.IsDBNull(5) ? 0 : Convert.ToInt32(r.GetValue(5));
                    return row;
                }
            }
        }
    }
}
