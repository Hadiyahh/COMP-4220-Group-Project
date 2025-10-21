using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Mail;

namespace BookStoreGUI
{
    /// <summary>
    /// Interaction logic for PaymentWindow.xaml
    /// </summary>
    public partial class PaymentWindow : Window
    {
        public PaymentWindow() // In here we only only put setup code
        {
            InitializeComponent(); // Loads everything we defined in the XAML
        }
        private void btnPay_Click(object sender, RoutedEventArgs e)
        {
            // optional to add: 
            // house address, postal code, city etc
            // for province I can have a drop down option with city names (we will keep our country location to be only Canada in that case)
            // Reciept option, or summary of order can be sprint 2
            // 
            // I want there to be a 'are you sure' when order is being placed with the total amount 
            string name = txtCardName.Text;
            string number = txtCardNumber.Text;
            string expiry = txtExpiry.Text;
            string cvv = txtCVV.Password;
            string email = txtEmail.Text;
            string address = txtAddress.Text;

            // If any of the fields are empty return error message

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(number) ||
               string.IsNullOrWhiteSpace(expiry) || string.IsNullOrWhiteSpace(cvv) ||
               string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(address))
            {
                MessageBox.Show("Please fill in all fields.", "Missing Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cvv.Length != 3 || number.Length != 16 || !expiry.Contains("/"))
            {
                MessageBox.Show("Please check card details.");
                return;
            }

            try
            {
                _ = new System.Net.Mail.MailAddress(email); // This line uses .NET’s built-in class MailAddress to check is the email is valid or not 
                // _ means we are only creating a object to test validity, we dont need to store it anywhere
            }
            catch
            {
                MessageBox.Show("Invalid email.");
                return;
            }

            // Validate address length
            if (address.Length < 5)
            {
                MessageBox.Show("Please enter a valid address.", "Invalid Address", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // All validation passed — show success to the user
            MessageBox.Show("Your order has been successfully placed.", "Payment Successful", MessageBoxButton.OK, MessageBoxImage.Information);

            // Later we can implement the order message to show the summary of the books ordered with the prices 

            // Clearing fields after successful payment
            // Better for security purposes (if poeple are looking at the screen)
            // Avoids duplicate submissions from the customer 
            txtCardName.Clear();
            txtExpiry.Clear();
            txtCVV.Clear();

            // Close the payment window after success
            this.Close();

        }

    }
}
