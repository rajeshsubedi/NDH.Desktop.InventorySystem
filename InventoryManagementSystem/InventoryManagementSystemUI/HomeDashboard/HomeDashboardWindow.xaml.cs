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

namespace InventoryManagementSystemUI.HomeDashboard
{
    public partial class HomeDashboardWindow : Window
    {
            public HomeDashboardWindow()
            {
                InitializeComponent();
            }

            private void Dashboard_Click(object sender, RoutedEventArgs e)
            {
                MessageBox.Show("Dashboard clicked");
            }

            private void Inventory_Click(object sender, RoutedEventArgs e)
            {
                MessageBox.Show("Inventory clicked");
            }

            private void Categories_Click(object sender, RoutedEventArgs e)
            {
                MessageBox.Show("Categories clicked");
            }

            private void Suppliers_Click(object sender, RoutedEventArgs e)
            {
                MessageBox.Show("Suppliers clicked");
            }

            private void Customers_Click(object sender, RoutedEventArgs e)
            {
                MessageBox.Show("Customers clicked");
            }

            private void Sales_Click(object sender, RoutedEventArgs e)
            {
                MessageBox.Show("Sales clicked");
            }

            private void Reports_Click(object sender, RoutedEventArgs e)
            {
                MessageBox.Show("Reports clicked");
            }

            private void Settings_Click(object sender, RoutedEventArgs e)
            {
                MessageBox.Show("Settings clicked");
            }

            private void Logout_Click(object sender, RoutedEventArgs e)
            {
                this.Close(); // or navigate to login
            }
        
    }
}
