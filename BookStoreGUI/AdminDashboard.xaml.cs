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
using BookStoreGUI;

namespace BookStoreGUI
{
    public partial class AdminDashboard : Window
    {
        public AdminDashboard() // Keeping this because this is our default constructor
        {
            InitializeComponent();
            // Optional: show inventory immediately when dashboard opens
            // Loaded += (_, __) => ContentHost.Content = new InventoryView();
        }
        public AdminDashboard(string username) : this() 
        {
            TxtCurrentUser.Text = $"Admin: {username}";
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e) => Close();

        private void NavInventory_Click(object sender, RoutedEventArgs e)
        {
            ContentHost.Content = new InventoryView();
        }

        private void NavCategories_Click(object sender, RoutedEventArgs e)
            => MessageBox.Show("Categories clicked (TODO)");

        private void NavOffers_Click(object sender, RoutedEventArgs e)
            => MessageBox.Show("Offers clicked (TODO)");

        private void NavOrders_Click(object sender, RoutedEventArgs e)
            => MessageBox.Show("Orders clicked (TODO)");
    }
}
