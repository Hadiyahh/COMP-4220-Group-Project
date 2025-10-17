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

        // could prob combine queries but its fine we just check manager bool with userid
        public bool GetManagerFlag(int userId) 
        {
            using (var conn = new SqlConnection(Properties.Settings.Default.dbConnectionString)) 
            using (var cmd = new SqlCommand("SELECT Manager FROM UserData WHERE UserID = @id", conn)) 
            {
                cmd.Parameters.AddWithValue("@id", userId); 
                conn.Open();
                var obj = cmd.ExecuteScalar(); 
                return (obj != null && obj != DBNull.Value) && Convert.ToBoolean(obj); 
            }
        } 

    }
}