/* **********************************************************************************
 * For use by students taking 60-422 (Fall, 2014) to work on assignments and project.
 * Permission required material. Contact: xyuan@uwindsor.ca 
 * **********************************************************************************/

using BookStoreLIB;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml.Linq;
using System.Configuration;

namespace BookStoreGUI
{
    /// Interaction logic for MainWindow.xaml
    public partial class MainWindow : Window
    {
        private UserData userData;

        private void registerButton_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder message
            MessageBox.Show("Register button clicked - Next implement backend logic",
                "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            userData = new UserData();
            var dlg = new LoginDialog();
            dlg.Owner = this;
            dlg.ShowDialog();

            if (dlg.DialogResult == true)
            {
                try
                {
                    if (userData.LogIn(dlg.nameTextBox.Text, dlg.passwordTextBox.Password))
                    {
                        statusTextBlock.Text = "You are logged in as: " + userData.LoginName;
                        loginButton.Visibility = Visibility.Collapsed;
                        logoutButton.Visibility = Visibility.Visible;
                        addButton.IsEnabled = true;
                        removeButton.IsEnabled = false;
                        clearCart.IsEnabled = false;
                    }
                    else
                    {
                        MessageBox.Show("You could not be verified. Please try again.");
                    }
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show(ex.Message); // show validation errors
                }
            }
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            //check if cart is not empty
            if (cart.cartBooks != null && cart.cartBooks.Count > 0)
            {
                //messagebox display to user
                var result = MessageBox.Show(
                    "Your cart is not empty. Would you like to clear the cart before logging out?",
                    "Confirm Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    clearCart_Click(sender, e);
                    PerformLogout();
                }
                //messageboxresult.no just do nothing
            }
            //cart is empty and user wants to logout
            else
            {
                PerformLogout();
            }
        }

        private void adminButton_Click(object sender, RoutedEventArgs e)
        {
            //new AdminDashboard().Show();
        }

        private void PerformLogout()
        {
            userData = null;
            statusTextBlock.Text = "Please login before proceeding to checkout.";
            loginButton.Visibility = Visibility.Visible;
            logoutButton.Visibility = Visibility.Collapsed;
            
            addButton.IsEnabled = false;
            statusTextBlock.Text = "You have been logged out.";
            statusTextBlock.Foreground = Brushes.Black;
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ProductsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        // to pull from list
        private List<Book> inventory = new List<Book>();
       
        private Cart cart = new Cart();
        public void LoadBooks()
        {  // name="BookStoreRemote"  ?
            var CString1 = "Data Source=tfs.cs.uwindsor.ca;Initial Catalog=Agile1422DB25;Persist Security Info=True;User ID=Agile1422U25;Password=Agile1422U25$;Encrypt=True;TrustServerCertificate=True";
                using (var conn1 = new SqlConnection(CString1))
            {
                conn1.Open();
                var SqlQue1 = "SELECT ISBN, CategoryID, Title, Author, Price, Year, InStock FROM BookData";

                using (var QueCmd1 = new SqlCommand(SqlQue1, conn1))
                using (var Reader1 = QueCmd1.ExecuteReader())
                {
                    while (Reader1.Read())

                        //Console.WriteLine("Title: " + Reader.GetFieldType(2));
                    {
                        var book = new Book
                        {
                            ISBN = Reader1.GetString(0),
                            CategoryID = Reader1.GetInt32(1),
                            Title = Reader1.GetString(2),
                            Author = Reader1.GetString(3),
                            Price = Reader1.GetDecimal(4),
                            Year = Reader1.GetString(5),
                            InStock = Reader1.GetInt32(6)
                        };

                        inventory.Add(book);
                    }
                }

            }
        }

        public void LoadCart()
        {  // name="BookStoreRemote"  ? 
            var CString2 = "Data Source=tfs.cs.uwindsor.ca;Initial Catalog=Agile1422DB25;Persist Security Info=True;User ID=Agile1422U25;Password=Agile1422U25$;Encrypt=True;TrustServerCertificate=True"; ;
            using (var conn2 = new SqlConnection(CString2))
            {
                conn2.Open();
                var SqlQue2 = "SELECT ISBN, Quantity, Subtotal FROM Cart";

                using (var QueCmd2 = new SqlCommand(SqlQue2, conn2))
                using (var Reader2 = QueCmd2.ExecuteReader())
                {
                    while (Reader2.Read())

                    {
                        var book = new Book
                        {
                            ISBN = Reader2.GetString(0),
                            Quantity = Reader2.GetInt32(1),
                            Subtotal = Reader2.GetDecimal(2),
                        };

                        cart.addBook(book);
                    }
                }

            }
        }

        public MainWindow() { // for books from a book list
           InitializeComponent();
           LoadBooks();
           LoadCart();
           ProductsDataGrid.ItemsSource = inventory;
           orderListView.ItemsSource = cart.cartBooks;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            addButton.IsEnabled = false;
            removeButton.IsEnabled = false;
            clearCart.IsEnabled = false;
        }
        private void updateCart() // for cart UI refresh
        {
            // cart.ExpiredBooks();
            orderListView.ItemsSource = null;
            orderListView.ItemsSource = cart.cartBooks;
        }

        private void addButton_Click(object sender, RoutedEventArgs e) // add button
        {
            Book bookChoice = (Book)ProductsDataGrid.SelectedItem;

            // handling if no book is selected
            if (bookChoice == null)
            {
                statusTextBlock.Text = "Error: Please select a book.";
                statusTextBlock.Foreground = Brushes.Red;
                return;
            }

            if (cart.addBook(bookChoice))
            {
                // pass to add book for boolean return
                updateCart();
                statusTextBlock.Text = "SUCCESS: Added to cart!";
                statusTextBlock.Foreground = Brushes.Green;
            }
            else
            {
                statusTextBlock.Text = "ERROR: Please try again.";
                statusTextBlock.Foreground = Brushes.Red;
            }
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            Book bookChoice = (Book)orderListView.SelectedItem;

            // handling if no book is selected
            if (bookChoice == null)
            {
                statusTextBlock.Text = "ERROR: Book not selected.";
                statusTextBlock.Foreground = Brushes.Red;
                return;
            }

            if (cart.removeBook(bookChoice))
            {
                updateCart();
                statusTextBlock.Text = "SUCCESS: Removed from cart!";
                statusTextBlock.Foreground = Brushes.Green;
            }
            else
            {
                statusTextBlock.Text = "ERROR: Unable to remove from cart.";
                statusTextBlock.Foreground = Brushes.Red;
            }
        }

        private void clearCart_Click(object sender, RoutedEventArgs e)
        {
            if (cart.cartBooks.Count == 0)
            {
                statusTextBlock.Text = "ERROR: Cart already empty.";
                statusTextBlock.Foreground = Brushes.Red;
                return;
            }
            cart.clearCart();
            updateCart();
            statusTextBlock.Text = "SUCCESS: Cart cleared!";
            statusTextBlock.Foreground = Brushes.Green;
        }
        private void checkoutButton_Click(object sender, RoutedEventArgs e) {
            if (cart.cartBooks.Count == 0)
            {
                MessageBox.Show("Your cart is empty.");
                return;
            }

            //var checkout = new CheckoutWindow(cart.shoppingCart)
            //{
            //    Owner = this
            //};

            //checkout.ShowDialog();
        }
    }
}
