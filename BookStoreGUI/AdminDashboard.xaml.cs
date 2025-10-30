using System.Windows;
using BookStoreGUI.Views;

namespace BookStoreGUI
{
    public partial class AdminDashboard : Window
    {
        public AdminDashboard()
        {
            InitializeComponent();
            // Optional: show inventory immediately when dashboard opens
            // Loaded += (_, __) => ContentHost.Content = new InventoryView();
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
