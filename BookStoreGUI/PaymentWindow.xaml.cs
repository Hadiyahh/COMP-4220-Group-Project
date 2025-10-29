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
using BookStoreLIB;

namespace BookStoreGUI
{
    /// <summary>
    /// Interaction logic for PaymentWindow.xaml
    /// </summary>
    public partial class PaymentWindow : Window
    {
        private List<Book> orderItems;
        private decimal orderTotal;
       // public PaymentWindow(List<Book> cartItems) // In here we only only put setup code
        private decimal subtotal;
        private decimal taxes;
        private decimal deliveryFee;
            public PaymentWindow(List<Book> cartItems, decimal subtotal, decimal taxes, decimal deliveryFee)
        {
            InitializeComponent(); // Loads everything we defined in the XAML
            orderItems = cartItems ?? new List<Book>();
        //  orderTotal = orderItems.Sum(book => book.Price * book.Quantity);
            this.subtotal = subtotal;
            this.taxes = taxes;
            this.deliveryFee = deliveryFee;
            this.orderTotal = subtotal + taxes + deliveryFee;
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

            var result = MessageBox.Show(
                $"Are you sure you want to place this order?\n\nTotal Amount: ${orderTotal:F2}",
                "Confirm Order",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Show order summary in message box
                ShowOrderSummary();

                // Clear fields and close window
                txtCardName.Clear();
                txtExpiry.Clear();
                txtCVV.Clear();
                this.Close();
            }

            /*
             Commented out clearing fields and added a payment summary
             */

            // All validation passed — show success to the user
            //MessageBox.Show("Your order has been successfully placed.", "Payment Successful", MessageBoxButton.OK, MessageBoxImage.Information);

            // Later we can implement the order message to show the summary of the books ordered with the prices 

            // Clearing fields after successful payment
            // Better for security purposes (if poeple are looking at the screen)
            // Avoids duplicate submissions from the customer 
            //txtCardName.Clear();
            //txtExpiry.Clear();
            //txtCVV.Clear();

            // Close the payment window after success
            //this.Close();

        }

        private void ShowOrderSummary()
        {
            string summary = "=== ORDER CONFIRMED ===\n" +
                           $"Order Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                           "----------------------------------------\n" +
                           "ITEMS ORDERED:\n\n";

            foreach (var book in orderItems)
            {
                string bookLine = $"{book.Title}";
                string priceLine = $"${book.Price * book.Quantity:F2}";

                // Add tabs to align the prices
                summary += $"{bookLine}\t{book.Quantity}\t{priceLine}\n";
            }

            summary += "----------------------------------------\n" +
                      $"SUBTOTAL:\t\t${subtotal:F2}\n" +
                      $"TAX (13%):\t\t${taxes:F2}\n" +
                      $"DELIVERY FEE:\t\t${deliveryFee:F2}\n" +
                      "----------------------------------------\n" +
                      $"TOTAL:\t\t\t${orderTotal:F2}\n" +
                      "\nThank you for your order!\n" +
                      "A confirmation email has been sent.";

            MessageBox.Show(summary, "Order Summary", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


    }
}
