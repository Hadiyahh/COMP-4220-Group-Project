/* **********************************************************************************
 * For use by students taking 60-422 (Fall, 2014) to work on assignments and project.
 * Permission required material. Contact: xyuan@uwindsor.ca 
 * **********************************************************************************/

using System;
using System.Windows;
using BookStoreLIB;

namespace BookStoreGUI
{
    public partial class RegisterDialog : Window
    {
        // Exposed so caller can read it after ShowDialog() == true
        public string CreatedUserName { get; private set; }
        public string CreatedPassword { get; private set; }

        public RegisterDialog()
        {
            InitializeComponent();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string fullName = fullNameTextBox.Text.Trim();
            string username = usernameTextBox.Text.Trim();
            string password = passwordBox.Password;             // Passwords may contain spaces; don’t Trim()
            string confirmPassword = confirmPasswordBox.Password;

            // Basic validation
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                MessageBox.Show("Passwords do not match.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var db = new DALUserInfo();
                bool success = db.RegisterUser(fullName, username, password);

                if (success)
                {
                    // ✅ Set this so the caller can read it (fixes CS1061 usage site)
                    CreatedUserName = username;
                    CreatedPassword = password;
                    MessageBox.Show("Account created successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;   // standard WPF pattern
                    Close();
                }
                else
                {
                    MessageBox.Show("Username already exists. Try another.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error registering user:\n{ex.Message}", "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
