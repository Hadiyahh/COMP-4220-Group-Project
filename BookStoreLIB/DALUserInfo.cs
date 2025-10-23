using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace BookStoreLIB
{
    internal class DALUserInfo
    {
        public int LogIn(string userName, string password)
        {
            var conn = new SqlConnection(Properties.Settings.Default.dbConnectionString);

            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "Select UserID from UserData where UserName = @UserName and Password = @Password";

                cmd.Parameters.AddWithValue("@UserName", userName);
                cmd.Parameters.AddWithValue("@Password", password);

                conn.Open();
                object result = cmd.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    int userID = Convert.ToInt32(result);
                    if (userID > 0) return userID;
                }

                return -1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return -1;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        // get manager flag and user type in one query
        public (bool IsManager, string Type) GetManagerAndType(int userId)
        {
            using (var conn = new SqlConnection(Properties.Settings.Default.dbConnectionString))
            using (var cmd = new SqlCommand(
                "SELECT CAST(Manager AS bit) AS Manager, [Type] " +
                "FROM UserData WHERE UserID = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", userId);
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        bool isManager = rdr.GetBoolean(0);
                        string type = rdr.IsDBNull(1) ? null : rdr.GetString(1);
                        return (isManager, type);
                    }
                }
            }
            return (false, null);
        }


    }
}