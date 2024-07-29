using LIM.EntityServices;
using LIM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LIM.Windows
{
    /// <summary>
    /// Interaction logic for InventoryItemWindow.xaml
    /// </summary>
    public partial class InventoryItemWindow : Window
    {
        public InventoryItem InventoryItem;

        public EntityManager<InventoryItem> InventoryItems { get; }

        public InventoryItemWindow(InventoryItem inventoryItem, EntityManager<InventoryItem> inventoryItems)
        {
            InitializeComponent();
            InventoryItem = inventoryItem;
            InventoryItems = inventoryItems;
            productGrid.DataContext = inventoryItem;
            stockContentBox.Text = InventoryItem.ActualInventory.ToString();
        }

        private void Button_Link_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            InventoryItems.SetUpdated(InventoryItem, true);
            Close();
        }

        private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            
        }

        private void Button_Print_Click(object sender, RoutedEventArgs e)
        {
        }

        private void cmdUp_Click(object sender, RoutedEventArgs e)
        {
            InventoryItem.ActualInventory += 1;
            UpdateStockContentBox();
        }

        private void UpdateStockContentBox()
        {
            stockContentBox.Text = InventoryItem.ActualInventory.ToString();
        }

        private void cmdDown_Click(object sender, RoutedEventArgs e)
        {
            InventoryItem.ActualInventory -= 1;
            UpdateStockContentBox();
        }

        private void stockContentBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.,-]+"); //regex that matches disallowed text
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
        }

        private void stockContentBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (InventoryItem == null) return;
            decimal parsedCurrentTextBoxValue = Decimal.Parse(stockContentBox.Text);
            if (parsedCurrentTextBoxValue != InventoryItem.ActualInventory)
            {
                InventoryItem.ActualInventory = parsedCurrentTextBoxValue;
            }
        }
    }
}
