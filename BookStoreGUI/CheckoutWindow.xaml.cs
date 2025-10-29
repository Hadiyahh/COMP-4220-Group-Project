using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using BookStoreLIB;

namespace BookStoreGUI
{
    public partial class CheckoutWindow : Window
    {
        private readonly List<Book> cartItems;

        public CheckoutWindow(List<Book> cartItems)
        {
            InitializeComponent();
            rdoStandard.IsChecked = true;
            this.cartItems = cartItems ?? new List<Book>();
            this.Loaded += (s,e) => LoadOrderSummary();
        }

        private void LoadOrderSummary()
        {
            if (rdoExpress == null || txtSubtotal == null) return;
            decimal subtotal = GetSubtotalFromCart();
            decimal taxes = Math.Round(subtotal * 0.13m, 2);
            decimal deliveryFee = (rdoExpress?.IsChecked == true) ? 5.00m : 0.00m;
            decimal total = subtotal + taxes + deliveryFee;
            txtSubtotal.Text = $"Subtotal: ${subtotal:F2}";
            txtTaxes.Text = $"Taxes (13%): ${taxes:F2}";
            txtDeliveryFee.Text = $"Delivery Fee: ${deliveryFee:F2}";
            txtTotal.Text = $"Total: ${total:F2}";
        }

        private decimal GetSubtotalFromCart()
        {
            if (cartItems == null || cartItems.Count == 0)
                return 0.00m;

            return cartItems.Sum(b => b.Price * b.Quantity);
        }

        private void rdoStandard_Checked(object sender, RoutedEventArgs e)
        {
            LoadOrderSummary();
        }

        private void rdoExpress_Checked(object sender, RoutedEventArgs e)
        {
            LoadOrderSummary();
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            decimal subtotal = GetSubtotalFromCart();
            decimal taxRate = 0.13m;
            decimal taxes = Math.Round(subtotal * taxRate, 2);
            decimal deliveryFee = rdoExpress.IsChecked == true ? 5.00m : 0.00m;
            decimal currentTotal = subtotal + taxes + deliveryFee;

            if (currentTotal <= 0)
            {
                MessageBox.Show("Your cart is empty.", "Cannot proceed to payment", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

			// for safe handling nulls
			string provinceValue = cmbShipProvince?.SelectedValue?.ToString()
                                   ?? cmbShipProvince?.SelectedItem?.ToString()
                                   ?? cmbShipProvince?.Text
                                   ?? string.Empty;

            var required = new Dictionary<string, string>
            {
                { "Name", txtShipFullName?.Text ?? string.Empty },
                { "Address", txtShipStreet?.Text ?? string.Empty },
                { "City", txtShipCity?.Text ?? string.Empty },
                { "Province", provinceValue },
                { "Postal Code", txtShipPostalCode?.Text ?? string.Empty }
            };

            var missing = required.Where(kv => string.IsNullOrWhiteSpace(kv.Value)).Select(kv => kv.Key).ToList();
            if (missing.Any())
            {
                MessageBox.Show($"Please fill in the following fields: {string.Join(", ", missing)}", "Missing Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Address format: at least two digits first, then at least two words
            const string addressPattern = @"^\s*\d{2,}\s+[A-Za-z]+\s+[A-Za-z]+(?:\s+.*)?$";
            string address = txtShipStreet?.Text ?? string.Empty;
            if (!Regex.IsMatch(address, addressPattern))
            {
                MessageBox.Show("Invalid address format.", "Invalid Address", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var paymentWindow = new PaymentWindow(cartItems, subtotal, taxes, deliveryFee)
            {
                Owner = this
            };

            this.Hide();
            paymentWindow.ShowDialog();
            this.Close();
        }
    }
}
