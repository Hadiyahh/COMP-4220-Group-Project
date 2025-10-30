using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace BookStoreLIB
{
    public class DALUserInfo
    {
        // -------------------- Connection resolution --------------------
        private static string ResolveConn()
        {
            var raw = ConfigurationManager.ConnectionStrings["BookStoreRemote"]?.ConnectionString;
            if (!string.IsNullOrWhiteSpace(raw))
            {
                var expanded = Environment.ExpandEnvironmentVariables(raw);
                var csb = new SqlConnectionStringBuilder(expanded);
                return csb.ConnectionString;
            }
            throw new InvalidOperationException(
                "Missing connection string. Define one of: 'BookStoreDBConnectionString', 'BookStoreRemote', or 'BookStoreLIB.Properties.Settings.dbConnectionString' in the startup/test App.config.");
        }

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

        // -------------------- Password helpers --------------------
        private static byte[] NewSalt(int size = 16)
        {
            var salt = new byte[size];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
            return salt;
        }

        private static byte[] Pbkdf2(string password, byte[] salt, int iterations = 100_000, int len = 32)
        {
            using (var kdf = new Rfc2898DeriveBytes(password ?? "", salt, iterations, HashAlgorithmName.SHA256))
                return kdf.GetBytes(len);
        }

        private static bool VerifyHash(string password, byte[] salt, byte[] expected)
        {
            if (salt == null || expected == null) return false;
            var actual = Pbkdf2(password, salt, 100_000, expected.Length);
            if (actual.Length != expected.Length) return false;
            // constant-time compare
            int d = 0;
            for (int i = 0; i < actual.Length; i++) d |= actual[i] ^ expected[i];
            return d == 0;
        }

        // -------------------- Login --------------------
        public int LogIn(string userName, string password)
        {
            using (var conn = new SqlConnection(ResolveConn()))
            {
                conn.Open();

                // Prefer hashed path if available
                using (var cmd = new SqlCommand(
                    "SELECT TOP 1 UserID, PasswordHash, PasswordSalt, [Password] FROM dbo.UserData WHERE UserName=@UserName", conn))
                {
                    cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 20).Value = userName ?? "";
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            int userId = r.GetInt32(0);
                            byte[] hash = r.IsDBNull(1) ? null : (byte[])r[1];
                            byte[] salt = r.IsDBNull(2) ? null : (byte[])r[2];

                            if (hash != null && salt != null)
                            {
                                if (VerifyHash(password, salt, hash)) return userId;
                                return -1;
                            }
                        }
                    }
                }

                // Legacy fallback (plaintext stored in [Password])
                using (var cmd2 = new SqlCommand(
                    "SELECT UserID FROM dbo.UserData WHERE UserName=@UserName AND [Password]=@Password", conn))
                {
                    cmd2.Parameters.Add("@UserName", SqlDbType.VarChar, 20).Value = userName ?? "";
                    cmd2.Parameters.Add("@Password", SqlDbType.VarChar, 25).Value = password ?? "";
                    object result = cmd2.ExecuteScalar();
                    return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : -1;
                }
            }
        }

        // -------------------- Role/Type flags (team-compat) --------------------
        public (bool IsManager, string Type) GetManagerAndType(int userId)
        {
            bool isMgr = false;
            string type = null;

            using (var conn = new SqlConnection(ResolveConn()))
            using (var cmd = new SqlCommand(
                "SELECT Manager, [Type] FROM dbo.UserData WHERE UserID=@UserID", conn))
            {
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        isMgr = rdr.GetBoolean(0);
                        type = rdr.IsDBNull(1) ? null : rdr.GetString(1);
                    }
                }
            }
            return (isMgr, type);
        }

        // optional Try* for future use
        public bool TryGetManagerAndType(int userId, out bool isManager, out string type)
        {
            var t = GetManagerAndType(userId);
            isManager = t.IsManager; type = t.Type;
            return (type != null) || isManager;
        }

        // -------------------- Register (keep team signature) --------------------
        public bool RegisterUser(string fullName, string username, string password)
        {
            return RegisterUser(fullName, username, password, null);
        }

        // -------------------- Register (new overload saves Email + hashed password) --------------------
        public bool RegisterUser(string fullName, string username, string password, string email)
        {
            using (var conn = new SqlConnection(ResolveConn()))
            {
                conn.Open();

                // Duplicate username check
                using (var check = new SqlCommand(
                    "SELECT COUNT(*) FROM dbo.UserData WHERE UserName=@UserName", conn))
                {
                    check.Parameters.Add("@UserName", SqlDbType.VarChar, 20).Value = username ?? "";
                    int exists = (int)check.ExecuteScalar();
                    if (exists > 0) return false;
                }

                // Prepare hash
                var salt = NewSalt(16);
                var hash = Pbkdf2(password, salt, 100_000, 32);

                // Insert: rely on the trigger to assign UserID
                string sql = email == null
                    ? "INSERT INTO dbo.UserData (FullName, UserName, [Password], [Type], Manager, PasswordHash, PasswordSalt) " +
                      "VALUES (@FullName, @UserName, @Masked, 'CU', 0, @Hash, @Salt)"
                    : "INSERT INTO dbo.UserData (FullName, UserName, [Password], [Type], Manager, Email, PasswordHash, PasswordSalt) " +
                      "VALUES (@FullName, @UserName, @Masked, 'CU', 0, @Email, @Hash, @Salt)";

                using (var insert = new SqlCommand(sql, conn))
                {
                    insert.Parameters.Add("@FullName", SqlDbType.NVarChar, 50).Value =
                        string.IsNullOrEmpty(fullName) ? (object)DBNull.Value : fullName;
                    insert.Parameters.Add("@UserName", SqlDbType.VarChar, 20).Value = username ?? "";
                    insert.Parameters.Add("@Masked", SqlDbType.VarChar, 25).Value = "***"; // never store plaintext
                    if (email != null)
                        insert.Parameters.Add("@Email", SqlDbType.VarChar, 50).Value = email;

                    var pHash = insert.Parameters.Add("@Hash", SqlDbType.VarBinary, 32);
                    pHash.Value = hash;
                    var pSalt = insert.Parameters.Add("@Salt", SqlDbType.VarBinary, 16);
                    pSalt.Value = salt;

                    insert.ExecuteNonQuery();
                }

                return true;
            }
        }
    }
}

//using System;
//using System.Data;
//using System.Data.SqlClient;
//using System.Configuration; 

//namespace BookStoreLIB
//{
//    internal class DALUserInfo
//    {
//        private static string ResolveConn()
//        {
//            // for dal we now use the connection string from config if present
//            // this allows us to use the remote db
//            var raw = ConfigurationManager.ConnectionStrings["BookStoreRemote"]?.ConnectionString;

//            if (!string.IsNullOrWhiteSpace(raw))
//            {
//                var expanded = Environment.ExpandEnvironmentVariables(raw);
//                var sb = new SqlConnectionStringBuilder(expanded);
//                return sb.ConnectionString;
//            }

//            var user = Environment.GetEnvironmentVariable("AGILE_DB_USER");
//            var pass = Environment.GetEnvironmentVariable("AGILE_DB_PASSWORD");
//            var server = Environment.GetEnvironmentVariable("AGILE_DB_SERVER") ?? "tfs.cs.uwindsor.ca";
//            var db = Environment.GetEnvironmentVariable("AGILE_DB_NAME") ?? "Agile1422DB25";

//            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
//                throw new InvalidOperationException("Missing AGILE_DB_USER / AGILE_DB_PASSWORD environment variables.");

//            var sb2 = new SqlConnectionStringBuilder
//            {
//                DataSource = server,
//                InitialCatalog = db,
//                PersistSecurityInfo = true,
//                UserID = user,
//                Password = pass,
//                Encrypt = true,
//                TrustServerCertificate = true
//            };
//            return sb2.ConnectionString;
//        }
//        // made it a bit safer 
//        public int LogIn(string userName, string password)
//        {
//            using (var conn = new SqlConnection(ResolveConn()))
//            using (var cmd = new SqlCommand(
//                "SELECT UserID FROM UserData WHERE UserName=@UserName AND Password=@Password", conn))
//            {
//                cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 256).Value = userName ?? "";
//                cmd.Parameters.Add("@Password", SqlDbType.NVarChar, 256).Value = password ?? "";

//                conn.Open();
//                var result = cmd.ExecuteScalar();
//                return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : -1;
//            }
//        }

//        // also made it a bit safer with type checks
//        public (bool IsManager, string Type) GetManagerAndType(int userId)
//        {
//            using (var conn = new SqlConnection(ResolveConn()))
//            using (var cmd = new SqlCommand(
//                "SELECT CAST(Manager AS bit), [Type] FROM UserData WHERE UserID=@id", conn))
//            {
//                cmd.Parameters.Add("@id", SqlDbType.Int).Value = userId;
//                conn.Open();
//                using (var rdr = cmd.ExecuteReader())
//                {
//                    if (rdr.Read())
//                        return (rdr.GetBoolean(0), rdr.IsDBNull(1) ? null : rdr.GetString(1));
//                }
//                return (false, null);
//            }
//        }
//    }
//}