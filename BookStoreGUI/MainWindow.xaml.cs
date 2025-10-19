/* **********************************************************************************
 * For use by students taking 60-422 (Fall, 2014) to work on assignments and project.
 * Permission required material. Contact: xyuan@uwindsor.ca 
 * **********************************************************************************/

using System;
using System.Diagnostics.Eventing.Reader;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using BookStoreLIB;
using System.Windows.Media;

namespace BookStoreGUI
{
    /// Interaction logic for MainWindow.xaml
    public partial class MainWindow : Window
    {
        private UserData userData;
        private void registerButton_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder message
            MessageBox.Show("Register button clicked - Next implement backend logic", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
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
            if (cart.shoppingCart != null && cart.shoppingCart.Count > 0)
            {
                //messagebox display to user
                var result = MessageBox.Show(
                    "Your cart is not empty. Would you like to clear the cart before logging out?",
                    "Confirm Logout",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

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

        private void exitButton_Click(object sender, RoutedEventArgs e) { this.Close(); }

        private void ProductsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { } // to pull from list

        private List<Book> inventory = new List<Book>();
        private List<Book> cartBooks = new List<Book>();
        public void LoadTestBooks() // sample hard coded books using Book class (TESTING)
        {
            inventory.Add(new Book { BookID = 101, Title = "Book A", Author = "Dr. Suess", Price = 10, Year = 1999 });
            inventory.Add(new Book { BookID = 102, Title = "Book B", Author = "John Doe", Price = 24, Year = 2008 });
            inventory.Add(new Book { BookID = 103, Title = "Book C", Author = "Jane Doe", Price = 38, Year = 2024 });
            inventory.Add(new Book { BookID = 104, Title = "Book D", Author = "Robert Munch", Price = 15, Year = 2017 });
            inventory.Add(new Book { BookID = 105, Title = "Book E", Author = "Kevin Smith", Price = 44, Year = 2008 });
            inventory.Add(new Book { BookID = 106, Title = "Book F", Author = "Willow Rich", Price = 38, Year = 2024 });
            inventory.Add(new Book { BookID = 107, Title = "Book G", Author = "Pricilla Esther", Price = 15, Year = 2017 });

        }
        public MainWindow() { // for books from a book list
            InitializeComponent();
            LoadTestBooks();
            ProductsDataGrid.ItemsSource = inventory;
            orderListView.ItemsSource = cartBooks;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            addButton.IsEnabled = false;
        }

        private Cart cart = new Cart(); // create a cart object
        private void updateCart() // for cart UI refresh
        {
           // cart.ExpiredBooks();
            orderListView.ItemsSource = null;
            orderListView.ItemsSource = cart.shoppingCart;
        }
        private void addButton_Click(object sender, RoutedEventArgs e) // add button
        {
            Book bookChoice = (Book)ProductsDataGrid.SelectedItem; // handling if no book is selected

            if (bookChoice == null) { 
                statusTextBlock.Text = "Error: Please select a book.";
                statusTextBlock.Foreground = Brushes.Red;
                return;
            }

            if (cart.addBook(bookChoice)) {  // pass to add book for boolean return
                updateCart(); 
                statusTextBlock.Text = "SUCCESS: Added to cart!";
                statusTextBlock.Foreground = Brushes.Green;
            } else {
                statusTextBlock.Text="ERROR: Please try again.";
                statusTextBlock.Foreground = Brushes.Red;
            }
        }
        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            Book bookChoice = (Book)orderListView.SelectedItem; // handling if no book is selected

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
            if (cart.shoppingCart.Count == 0)
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
            if (cart.shoppingCart.Count == 0)
            {
                MessageBox.Show("Your cart is empty.");
                return;
            }
            // When i want to pass the cart contents to the payment window so it can show a summary i will add cart.shoppingCart in the parantheses
            var pay = new PaymentWindow();
            pay.ShowDialog();
        } 
        private void checkoutButton_Click(object sender, RoutedEventArgs e) { } // checkout method later?


    }
}