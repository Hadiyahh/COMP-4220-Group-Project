using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace BookStoreGUI
{
    public partial class AdminDashboardWindow : Window
    {
        private readonly string _cs;
        private readonly int _userId;
        private readonly string _loginName;

        
        public AdminDashboardWindow()
            : this(
                "Data Source=tfs.cs.uwindsor.ca;Initial Catalog=Agile1422DB25;Persist Security Info=True;User ID=Agile1422U25;Password=Agile1422U25$;Encrypt=True;TrustServerCertificate=True",
                0,
                "Admin")
        {
        }

        public AdminDashboardWindow(string connectionString, int userId, string loginName)
        {
            InitializeComponent();
            _cs = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _userId = userId;
            _loginName = loginName;

            // Optional: Title tweak
            this.Title = $"Admin Dashboard — {_loginName}";

            // Load data right away
            TryLoadInventory();
            TryLoadOrders();
        }

        // -------------------- DATA LOADING (no repository) --------------------

        private void TryLoadInventory()
        {
            try
            {
                using (var conn = new SqlConnection(_cs))
                using (var da = new SqlDataAdapter(@"
                SELECT  b.ISBN,
                        b.Title,
                        b.Author,
                        c.CategoryID AS Category,
                        b.Price,
                        b.InStock      AS Quantity
                FROM dbo.BookData b
                LEFT JOIN dbo.Category c   ON c.CategoryID = b.CategoryID 
                ORDER BY b.Title;", conn))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    InventoryGrid.ItemsSource = dt.DefaultView; // DataGrid from your XAML
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load inventory:\n" + ex.Message,
                    "Inventory Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TryLoadOrders()
        {
            try
            {
                using (var conn = new SqlConnection(_cs))
                using (var da = new SqlDataAdapter(@"
                    SELECT o.OrderID, o.UserID, o.OrderDate, o.Status,
                           oi.ISBN, oi.Quantity, oi.UnitPrice
                    FROM dbo.Orders o
                    LEFT JOIN dbo.OrderItem oi ON o.OrderID = oi.OrderID
                    ORDER BY o.OrderDate DESC, o.OrderID DESC;", conn))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    OrdersGrid.ItemsSource = dt.DefaultView; // DataGrid from your XAML
                }
            }
            catch (Exception ex)
            {
                // Don’t block dashboard if orders aren’t ready
                Console.WriteLine("Orders load warning: " + ex.Message);
                OrdersGrid.ItemsSource = null;
            }
        }

        // -------------------- BUTTON HANDLERS (stubs you can flesh out) --------------------

        private void RefreshInventory_Click(object sender, RoutedEventArgs e) => TryLoadInventory();
        private void RefreshOrders_Click(object sender, RoutedEventArgs e) => TryLoadOrders();
        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("TODO: INSERT INTO dbo.Books(...) VALUES(...);");
            TryLoadInventory();
        }

        private void EditBook_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("TODO: UPDATE dbo.Books SET ... WHERE ISBN = ...;");
            TryLoadInventory();
        }

        private void DeleteBook_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("TODO: DELETE FROM dbo.Books WHERE ISBN = ...;");
            TryLoadInventory();
        }

        private void CompleteOrder_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("TODO: UPDATE dbo.Orders SET Status='C' WHERE OrderID=@id;");
            TryLoadOrders();
        }

        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("TODO: DELETE FROM dbo.OrderItem WHERE OrderID=@id; DELETE FROM dbo.Orders WHERE OrderID=@id;");
            TryLoadOrders();
        }
    }
}
