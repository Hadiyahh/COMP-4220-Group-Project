using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration; 

namespace BookStoreLIB
{
    internal class DALUserInfo
    {
        private static string ResolveConn()
        {
            // for dal we now use the connection string from config if present
            // this allows us to use the remote db
            var raw = ConfigurationManager.ConnectionStrings["BookStoreRemote"]?.ConnectionString;

            if (!string.IsNullOrWhiteSpace(raw))
            {
                var expanded = Environment.ExpandEnvironmentVariables(raw);
                var sb = new SqlConnectionStringBuilder(expanded);
                return sb.ConnectionString;
            }

            var user = Environment.GetEnvironmentVariable("AGILE_DB_USER");
            var pass = Environment.GetEnvironmentVariable("AGILE_DB_PASSWORD");
            var server = Environment.GetEnvironmentVariable("AGILE_DB_SERVER") ?? "tfs.cs.uwindsor.ca";
            var db = Environment.GetEnvironmentVariable("AGILE_DB_NAME") ?? "Agile1422DB25";

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
                throw new InvalidOperationException("Missing AGILE_DB_USER / AGILE_DB_PASSWORD environment variables.");

            var sb2 = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = db,
                PersistSecurityInfo = true,
                UserID = user,
                Password = pass,
                Encrypt = true,
                TrustServerCertificate = true
            };
            return sb2.ConnectionString;
        }
        // made it a bit safer 
        public int LogIn(string userName, string password)
        {
            using (var conn = new SqlConnection(ResolveConn()))
            using (var cmd = new SqlCommand(
                "SELECT UserID FROM UserData WHERE UserName=@UserName AND Password=@Password", conn))
            {
                cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 256).Value = userName ?? "";
                cmd.Parameters.Add("@Password", SqlDbType.NVarChar, 256).Value = password ?? "";

                conn.Open();
                var result = cmd.ExecuteScalar();
                return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : -1;
            }
        }

        // also made it a bit safer with type checks
        public (bool IsManager, string Type) GetManagerAndType(int userId)
        {
            using (var conn = new SqlConnection(ResolveConn()))
            using (var cmd = new SqlCommand(
                "SELECT CAST(Manager AS bit), [Type] FROM UserData WHERE UserID=@id", conn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = userId;
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                        return (rdr.GetBoolean(0), rdr.IsDBNull(1) ? null : rdr.GetString(1));
                }
                return (false, null);
            }
        }
    }
}
