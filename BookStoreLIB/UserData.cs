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
                IsManager = dbUser.GetManagerFlag(UserID);
                return true;
            }
            else
            {
                LoggedIn = false;
                IsManager = false;
                return false;
            }
        }
    }
}
