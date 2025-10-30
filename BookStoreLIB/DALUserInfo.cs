using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Security.Cryptography;

namespace BookStoreLIB
{
    public class DALUserInfo
    {
        private readonly string connStr;

        public DALUserInfo()
        {
            // Try multiple keys so both GUI and tests work
            connStr = ResolveConnString(
                "BookStoreDBConnectionString",
                "BookStoreRemote",
                "BookStoreLIB.Properties.Settings.dbConnectionString" // LocalDB dev fallback
            );
        }

        private static string ResolveConnString(params string[] keys)
        {
            foreach (var k in keys)
            {
                var cs = ConfigurationManager.ConnectionStrings[k]?.ConnectionString;
                if (!string.IsNullOrWhiteSpace(cs))
                    return Environment.ExpandEnvironmentVariables(cs);
            }
            throw new InvalidOperationException(
                "Missing connection string. Define one of: 'BookStoreDBConnectionString', 'BookStoreRemote', or 'BookStoreLIB.Properties.Settings.dbConnectionString' in the startup/test App.config.");
        }

        // ===== PBKDF2 helpers =====
        private const int SaltSize = 16;   // 128-bit
        private const int HashSize = 32;   // 256-bit
        private const int Iterations = 10000;

        private static (byte[] hash, byte[] salt) HashPassword(string password)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var salt = new byte[SaltSize];
                rng.GetBytes(salt);
                // Rfc2898DeriveBytes without explicit algo works on .NET Framework (HMACSHA1)
                using (var pbkdf2 = new Rfc2898DeriveBytes(password ?? "", salt, Iterations))
                {
                    var hash = pbkdf2.GetBytes(HashSize);
                    return (hash, salt);
                }
            }
        }

        private static bool VerifyPassword(string password, byte[] hash, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password ?? "", salt, Iterations))
            {
                var cand = pbkdf2.GetBytes(HashSize);
                int diff = 0;
                for (int i = 0; i < cand.Length; i++) diff |= cand[i] ^ hash[i];
                return diff == 0;
            }
        }

        // ---------------- LOGIN (hash verify + legacy fallback) ----------------
        public int LogIn(string userName, string password)
        {
            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand(
                @"SELECT UserID, PasswordHash, PasswordSalt, [Password]
                  FROM dbo.UserData
                  WHERE UserName=@u;", conn))
            {
                cmd.Parameters.Add("@u", SqlDbType.NVarChar, 50).Value = userName ?? "";

                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return -1;

                        int userId = r.GetInt32(0);
                        bool hasHash = !r.IsDBNull(1) && !r.IsDBNull(2);
                        byte[] hash = hasHash ? (byte[])r.GetValue(1) : null;
                        byte[] salt = hasHash ? (byte[])r.GetValue(2) : null;
                        string legacyPw = r.IsDBNull(3) ? null : r.GetString(3);

                        // 1) New path: verify hash/salt
                        if (hasHash && VerifyPassword(password, hash, salt))
                            return userId;

                        // 2) Legacy fallback: plaintext match -> migrate row
                        if (!hasHash && !string.IsNullOrEmpty(legacyPw) && legacyPw == (password ?? ""))
                        {
                            var hp = HashPassword(password);
                            byte[] newHash = hp.hash;
                            byte[] newSalt = hp.salt;

                            r.Close(); // must close reader before UPDATE

                            using (var up = new SqlCommand(
                                @"UPDATE dbo.UserData
                                  SET [Password]=N'***', PasswordHash=@h, PasswordSalt=@s
                                  WHERE UserID=@id;", conn))
                            {
                                up.Parameters.Add("@h", SqlDbType.VarBinary, HashSize).Value = newHash;
                                up.Parameters.Add("@s", SqlDbType.VarBinary, SaltSize).Value = newSalt;
                                up.Parameters.Add("@id", SqlDbType.Int).Value = userId;
                                up.ExecuteNonQuery();
                            }
                            return userId;
                        }

                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    return -1;
                }
            }
        }

        // ------------- MANAGER + TYPE ----------
        public (bool IsManager, string Type) GetManagerAndType(int userId)
        {
            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand(
                "SELECT CAST(Manager AS bit) AS Manager, [Type] FROM dbo.UserData WHERE UserID=@id;", conn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = userId;

                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        bool isManager = r.GetBoolean(0);
                        string type = r.IsDBNull(1) ? null : r.GetString(1);
                        return (isManager, type);
                    }
                }
            }
            return (false, null);
        }

        // ------------- REGISTER (3 args) -------
        public bool RegisterUser(string fullName, string username, string password)
            => RegisterUser(fullName, username, password, null);

        // ------------- REGISTER (4 args) -------
        public bool RegisterUser(string fullName, string username, string password, string email)
        {
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
                return false;

            // Trim and normalize input
            string f = fullName.Trim();
            string u = username.Trim();
            string e = string.IsNullOrWhiteSpace(email) ? null : email.Trim();

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                // Duplicate username (normalize same as insert)
                using (var check = new SqlCommand(
                    "SELECT COUNT(*) FROM dbo.UserData WHERE LOWER(RTRIM(UserName)) = LOWER(RTRIM(@u));", conn))
                {
                    check.Parameters.Add("@u", SqlDbType.NVarChar, 50).Value = u;
                    int exists = (int)check.ExecuteScalar();
                    if (exists > 0) return false;
                }

                // Hash + salt
                var hp = HashPassword(password);
                byte[] hash = hp.hash;
                byte[] salt = hp.salt;

                // Insert new user
                using (var ins = new SqlCommand(
                    @"INSERT INTO dbo.UserData
                      (FullName, UserName, [Password], PasswordHash, PasswordSalt, Email, [Type], Manager)
                      VALUES
                      (@f, @u, N'***', @h, @s, @e, N'CU', 0);", conn))
                {
                    ins.Parameters.Add("@f", SqlDbType.NVarChar, 100).Value = f;
                    ins.Parameters.Add("@u", SqlDbType.NVarChar, 50).Value = u;
                    ins.Parameters.Add("@h", SqlDbType.VarBinary, HashSize).Value = hash;
                    ins.Parameters.Add("@s", SqlDbType.VarBinary, SaltSize).Value = salt;
                    ins.Parameters.Add("@e", SqlDbType.NVarChar, 100).Value = (object)e ?? DBNull.Value;
                    ins.ExecuteNonQuery();
                }

                return true;
            }
        }
    }
}
