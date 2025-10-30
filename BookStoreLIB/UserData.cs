using System;

namespace BookStoreLIB
{
    public class UserData
    {
        public int UserID { set; get; }
        public string LoginName { set; get; }
        public string Password { set; get; }
        public Boolean LoggedIn { set; get; }
        public bool IsManager { get; private set; }
        public string Type { get; private set; }
        public Boolean LogIn(string loginName, string passWord)
        {
            // Step 1: Basic validation
            if (string.IsNullOrEmpty(loginName) || string.IsNullOrEmpty(passWord))
            {
                throw new ArgumentException("Please fill in all slots.");
            }

            bool hasLetter = false, hasDigit = false;
            foreach (char c in passWord)
            {
                if (Char.IsLetter(c)) hasLetter = true;
                else if (Char.IsDigit(c)) hasDigit = true;
                else
                {
                    throw new ArgumentException("A valid password can only contain letters and numbers.");
                }
            }

            // Step 2: Password format checks
            if (passWord.Length < 6)
            {
                throw new ArgumentException("A valid password needs to have at least six characters with both letters and numbers.");
            }

            if (!Char.IsLetter(passWord[0]))
            {
                throw new ArgumentException("A valid password needs to start with a letter.");
            }

            if (!hasLetter || !hasDigit)
            {
                throw new ArgumentException("A valid password needs to have at least six characters with both letters and numbers.");
            }

            // Step 3: DB lookup
            var dbUser = new DALUserInfo();
            UserID = dbUser.LogIn(loginName, passWord);

            if (UserID > 0)
            {
                LoginName = loginName;
                Password = passWord;
                LoggedIn = true;
                var flags = dbUser.GetManagerAndType(UserID);   
                IsManager = flags.IsManager;                    
                Type = flags.Type;
                return true;
            }
            else
            {
                LoggedIn = false;
                IsManager = false;
                Type = null;
                return false;
            }
        }

        private static bool ValidateRegistration(string username, string password, string confirmPassword, string email)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword) ||
                string.IsNullOrWhiteSpace(email)) return false;

            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal)) return false;
            if (password.Length < 6) return false;
            if (!char.IsLetter(password[0])) return false;

            bool hasLetter = false, hasDigit = false;
            foreach (char c in password)
            {
                if (char.IsLetter(c)) hasLetter = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else return false; // only letters/digits
            }
            if (!hasLetter || !hasDigit) return false;

            if (!email.Contains("@") || email.StartsWith("@") || email.EndsWith("@")) return false;

            return true;
        }

        // 1) INSTANCE overload (ud.RegisterUser(u,p,c,e))
        public bool RegisterUser(string username, string password, string confirmPassword, string email)
        {
            return ValidateRegistration(username, password, confirmPassword, email);
        }

        // 2) STATIC overload (UserData.RegisterUser(u,p,c,e))
        public static bool RegisterUser(string username, string password, string confirmPassword, string email, bool _ = false)
        {
            // note: extra optional bool only to avoid any ambiguity with instance method groups in some test harnesses
            return ValidateRegistration(username, password, confirmPassword, email);
        }


    }
}
