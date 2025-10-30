/* **********************************************************************************
 * For use by students taking 60-422 (Fall, 2014) to work on assignments and project.
 * Permission required material. Contact: xyuan@uwindsor.ca
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BookStoreLIB;                 // Book, Cart, UserData, etc.
using System.Configuration;
using BookStoreGUI;

namespace BookStoreGUI
{
    /// Interaction logic for MainWindow.xaml
    public partial class MainWindow : Window
    {
        // ===== App state =====
        private UserData userData;
        private readonly List<Book> inventory = new List<Book>();
        private readonly Cart cart = new Cart();

        // Optional repo/view wiring for future Admin/Inventory work.
        // Safe to leave even if you don’t have these views yet.
        private static bool IsAdmin(UserData u) =>
            u != null && string.Equals(u.Type, "AD", StringComparison.OrdinalIgnoreCase);

        private void TryShowInventory()
        {
            try
            {
                // Requires a <ContentControl x:Name="ContentHost" .../> in XAML
                // and a Views/InventoryView user control.
                var view = new Views.InventoryView();
                // If InventoryView binds to a collection:
                view.DataContext = inventory;
                //ContentHost.Content = view;
            }
            catch
            {
                // Silently ignore if ContentHost or view doesn’t exist.
            }
        }

        private void InventoryButton_Click(object sender, RoutedEventArgs e)
        {
            TryShowInventory();
        }

        // ===== Constructor =====
        public MainWindow()
        {
            InitializeComponent();
            LoadBooks();
            LoadCart();

            // Bind UI
            ProductsDataGrid.ItemsSource = inventory;
            orderListView.ItemsSource = cart.cartBooks;

            // Disable actions until login
            addButton.IsEnabled = false;
            removeButton.IsEnabled = false;
            clearCart.IsEnabled = false;

            // If you want to show inventory view by default and you have ContentHost:
            // TryShowInventory();
        }

        // ===== Window lifecycle =====
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Keep if XAML still hooks this
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // ===== Auth =====
        private void registerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new RegisterDialog { Owner = this };
                var ok = dlg.ShowDialog();

                if (ok == true && !string.IsNullOrEmpty(dlg.CreatedUserName))
                {
                    // Auto-login using the same path as loginButton_Click
                    userData = new UserData();
                    if (userData.LogIn(dlg.CreatedUserName, dlg.CreatedPassword))
                    {
                        statusTextBlock.Text = "You are logged in as: " + userData.LoginName;
                        loginButton.Visibility = Visibility.Collapsed;
                        logoutButton.Visibility = Visibility.Visible;
                        addButton.IsEnabled = true;
                    }
                    else
                    {
                        MessageBox.Show("Registered, but auto-login failed. Please log in manually.");
                    }
                }
                // else: user cancelled dialog; do nothing
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open registration: " + ex.Message);
            }
        }




        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            userData = new UserData();

            var dlg = new LoginDialog { Owner = this };
            if (dlg.ShowDialog() != true) return;

            // Make sure you’re reading the correct controls
            var username = dlg.nameTextBox.Text.Trim();   // <- ensure it’s the USERNAME box
            var password = dlg.passwordTextBox.Password;      // do NOT Trim() passwords

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Enter both username and password.");
                return;
            }

            try
            {
                if (!userData.LogIn(username, password))
                {
                    if (userData.LogIn(dlg.nameTextBox.Text, dlg.passwordTextBox.Password))
                    {
                        statusTextBlock.Text = "You are logged in as: " + userData.LoginName;
                        loginButton.Visibility = Visibility.Collapsed;
                        logoutButton.Visibility = Visibility.Visible;
                        addButton.IsEnabled = true;
                        removeButton.IsEnabled = true;
                        clearCart.IsEnabled = true;
                    }
                    if (userData.IsManager || string.Equals(userData.Type, "Admin", StringComparison.OrdinalIgnoreCase))
                    {
                        var dashboard = new AdminDashboard(userData.LoginName) { Owner = this };
                        this.Hide();                          // hide main window while dashboard is open
                        dashboard.Closed += (_, __) => this.Show(); // show it again when dashboard closes
                        dashboard.Show();                     // open dashboard
                    }
                
                    else
                    {
                        MessageBox.Show("You could not be verified. Please try again.");
                    }
                }

                statusTextBlock.Text = "You are logged in as: " + userData.LoginName;
                loginButton.Visibility = Visibility.Collapsed;
                logoutButton.Visibility = Visibility.Visible;
                addButton.IsEnabled = true;
                removeButton.IsEnabled = true;
                clearCart.IsEnabled = true;

                if (IsAdmin(userData))
                {
                    var admin = new AdminDashboard { Owner = this };
                    Hide();
                    admin.Closed += (_, __) => Show();
                    admin.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login error: " + ex.Message);
            }
        }




        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (cart.cartBooks != null && cart.cartBooks.Count > 0)
            {
                var result = MessageBox.Show(
                    "Your cart is not empty. Would you like to clear the cart before logging out?",
                    "Confirm Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    clearCart_Click(sender, e);
                    PerformLogout();
                }
            }
            else
            {
                PerformLogout();
            }
        }

        private void PerformLogout()
        {
            userData = null;
            statusTextBlock.Text = "You have been logged out.";
            statusTextBlock.Foreground = Brushes.Black;

            loginButton.Visibility = Visibility.Visible;
            logoutButton.Visibility = Visibility.Collapsed;

            addButton.IsEnabled = false;
            removeButton.IsEnabled = false;
            clearCart.IsEnabled = false;
        }

        private void adminButton_Click(object sender, RoutedEventArgs e)
        {
            // Open Admin dashboard window if present
            try
            {
                var admin = new AdminDashboard { Owner = this };
                admin.Show();
            }
            catch
            {
                MessageBox.Show("Admin dashboard is not available in this build.");
            }
        }

        // ===== Data loading (DB) =====
        public void LoadBooks()
        {
            // NOTE: this is the course server connection string the repo uses.
            var CString1 =
                "Data Source=tfs.cs.uwindsor.ca;Initial Catalog=Agile1422DB25;Persist Security Info=True;User ID=Agile1422U25;Password=Agile1422U25$;Encrypt=True;TrustServerCertificate=True";

            using (var conn1 = new SqlConnection(CString1))
            {
                conn1.Open();
                const string SqlQue1 = "SELECT ISBN, CategoryID, Title, Author, Price, Year, InStock FROM BookData";

                using (var cmd = new SqlCommand(SqlQue1, conn1))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        var book = new Book
                        {
                            ISBN = r.GetString(0),
                            CategoryID = r.GetInt32(1),
                            Title = r.GetString(2),
                            Author = r.GetString(3),
                            Price = r.GetDecimal(4),
                            Year = r.GetString(5),
                            InStock = r.GetInt32(6)
                        };
                        inventory.Add(book);
                    }
                }
            }
        }

        public void LoadCart()
        {
            var CString2 =
                "Data Source=tfs.cs.uwindsor.ca;Initial Catalog=Agile1422DB25;Persist Security Info=True;User ID=Agile1422U25;Password=Agile1422U25$;Encrypt=True;TrustServerCertificate=True";

            using (var conn2 = new SqlConnection(CString2))
            {
                conn2.Open();
                const string SqlQue2 = "SELECT ISBN, Quantity, Subtotal FROM Cart";

                using (var cmd = new SqlCommand(SqlQue2, conn2))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        var book = new Book
                        {
                            ISBN = r.GetString(0),
                            Quantity = r.GetInt32(1),
                            Subtotal = r.GetDecimal(2),
                        };
                        cart.addBook(book);
                    }
                }
            }
        }

        // ===== UI events for product/catalog =====
        private void ProductsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // No-op; kept so XAML handlers still compile
        }

        // ===== Cart helpers & events =====
        private decimal GetSubTotal()
        {
            decimal subtotal = 0;
            foreach (var book in cart.cartBooks)
            {
                subtotal += book.Price * book.Quantity;
            }
            subtotalTextBlock.Text = $"Subtotal: ${subtotal:F2}";
            return subtotal;
        }

        private void updateCart()
        {
            orderListView.ItemsSource = null;
            orderListView.ItemsSource = cart.cartBooks;
            _ = GetSubTotal();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            var bookChoice = (Book)ProductsDataGrid.SelectedItem;

            if (bookChoice == null)
            {
                statusTextBlock.Text = "Error: Please select a book.";
                statusTextBlock.Foreground = Brushes.Red;
                return;
            }

            if (cart.addBook(bookChoice))
            {
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
            var bookChoice = (Book)orderListView.SelectedItem;

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

        private void checkoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (cart.cartBooks.Count == 0)
            {
                MessageBox.Show("Your cart is empty.");
                return;
            }

            var checkout = new CheckoutWindow(cart.cartBooks)
            {
                Owner = this
            };

            //    checkout.ShowDialog();
        }
    }
}
