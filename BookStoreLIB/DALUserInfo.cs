using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Configuration;

namespace BookStoreLIB
{
    public class DALUserInfo   // ✅ Made public so GUI can access it
    {
        private readonly string connStr;

        public DALUserInfo()
        {
            // ✅ Reads from App.config (BookStoreDBConnectionString)
            connStr = ConfigurationManager.ConnectionStrings["BookStoreDBConnectionString"].ConnectionString;
        }

        // ---------------- LOGIN METHOD (TEAM CODE) ----------------
        public int LogIn(string userName, string password)
        {
            using (var conn = new SqlConnection(connStr))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT UserID FROM UserData WHERE UserName = @UserName AND Password = @Password";

                    cmd.Parameters.AddWithValue("@UserName", userName);
                    cmd.Parameters.AddWithValue("@Password", password);

                    conn.Open();
                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        int userID = Convert.ToInt32(result);
                        if (userID > 0)
                            return userID;
                    }

                    return -1;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    return -1;
                }
            }
        }

        // ---------------- GET MANAGER FLAG (TEAM CODE) ----------------
        public (bool IsManager, string Type) GetManagerAndType(int userId)
        {
            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand(
                "SELECT CAST(Manager AS bit) AS Manager, [Type] FROM UserData WHERE UserID = @id", conn))
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

        // ---------------- REGISTER USER (RITIKA’S ADDITION) ----------------
        public bool RegisterUser(string fullName, string username, string password)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // 1️⃣ Check if username already exists
                SqlCommand checkCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM UserData WHERE UserName = @UserName", conn);
                checkCmd.Parameters.AddWithValue("@UserName", username);
                int exists = (int)checkCmd.ExecuteScalar();

                if (exists > 0)
                    return false; // Username already taken

                // 2️⃣ Insert new record into UserData
                SqlCommand insertCmd = new SqlCommand(
                    "INSERT INTO UserData (FullName, UserName, Password, Type, Manager) " +
                    "VALUES (@FullName, @UserName, @Password, 'CU', 0)", conn);
                insertCmd.Parameters.AddWithValue("@FullName", fullName);
                insertCmd.Parameters.AddWithValue("@UserName", username);
                insertCmd.Parameters.AddWithValue("@Password", password);

                insertCmd.ExecuteNonQuery();
                return true; // ✅ Registration successful
            }
        }
    }
}
