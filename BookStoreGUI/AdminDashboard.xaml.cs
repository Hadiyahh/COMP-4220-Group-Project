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
        }
        public AdminDashboard(string username) : this() 
        {
            TxtCurrentUser.Text = $"Admin: {username}";
        }

        // Header: Logout
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Logout clicked (TODO: implement sign-out)");
            Close();
        }

        // Left Nav
        private void NavInventory_Click(object sender, RoutedEventArgs e)
        {
            // TODO: load Inventory view into ContentHost
            MessageBox.Show("Inventory clicked (TODO)");
        }

        private void NavCategories_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Categories clicked (TODO)");
        }

        private void NavOffers_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Offers clicked (TODO)");
        }

        private void NavOrders_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Orders clicked (TODO)");
        }
    }
}
