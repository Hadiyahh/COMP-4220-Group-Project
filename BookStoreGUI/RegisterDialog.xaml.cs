using System;
using System.Text.RegularExpressions;
using System.Windows;
using BookStoreLIB;

namespace BookStoreGUI
{
    public partial class RegisterDialog : Window
    {
        public string CreatedUserName { get; private set; }
        public string CreatedPassword { get; private set; }

        private static readonly Regex UsernameRx =
            new Regex(@"^[A-Za-z][A-Za-z0-9]{5,}$", RegexOptions.Compiled);
        private static readonly Regex EmailRx =
            new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        public RegisterDialog()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            var fullName = (nameTextBox.Text ?? "").Trim();
            var username = (usernameTextBox.Text ?? "").Trim();
            var email = (emailTextBox.Text ?? "").Trim();
            var pass = passwordBox.Password ?? "";
            var confirm = confirmPasswordBox.Password ?? "";

            if (fullName.Length == 0 || username.Length == 0 ||
                email.Length == 0 || pass.Length == 0 || confirm.Length == 0)
            {
                MessageBox.Show("All fields are required.");
                return;
            }
            if (!UsernameRx.IsMatch(username))
            {
                MessageBox.Show("Username must start with a letter, be at least 6 characters, and contain only letters and digits.");
                return;
            }
            if (!EmailRx.IsMatch(email))
            {
                MessageBox.Show("Please enter a valid email address.");
                return;
            }
            if (!string.Equals(pass, confirm, StringComparison.Ordinal))
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            try
            {
                var dal = new DALUserInfo();
                var ok = dal.RegisterUser(fullName, username, pass, email);
                if (!ok)
                {
                    MessageBox.Show("Username already exists. Try a different one.");
                    return;
                }

                CreatedUserName = username;
                CreatedPassword = pass;

                MessageBox.Show("Account created successfully!");
                DialogResult = true;
                Close();
            }
            catch
            {
                MessageBox.Show("Registration failed. Please try again.");
            }
        }
    }
}
