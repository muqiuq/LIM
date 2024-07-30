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
        private readonly LimAppContext appContext;

        private bool MarkedForUpload = false;

        public EntityManager<InventoryItem> InventoryItems { get; }

        public InventoryItemWindow(InventoryItem inventoryItem, LimAppContext appContext)
        {
            InitializeComponent();
            InventoryItem = inventoryItem;
            this.appContext = appContext;
            InventoryItems = appContext.InventoryItems;
            productGrid.DataContext = inventoryItem;
            stockContentBox.Text = InventoryItem.ActualInventory.ToString();
            LastFocused = DateTime.Now;
            InventoryItems.Lock(inventoryItem);
        }

        public DateTime LastFocused { get; private set; }

        private void Button_Link_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            CloseAndUpload();
        }

        public void CloseAndUpload()
        {
            InventoryItems.SetUpdated(InventoryItem, true);
            MarkedForUpload = true;
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
            AddOrRemove(1);
        }

        public void AddOrRemove(decimal amount = 0)
        {
            InventoryItem.ActualInventory += amount;
            UpdateStockContentBox();
        }

        private void UpdateStockContentBox()
        {
            stockContentBox.Text = InventoryItem.ActualInventory.ToString();
        }

        private void cmdDown_Click(object sender, RoutedEventArgs e)
        {
            AddOrRemove(-1);
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

        private void Button_Click_AddEAN(object sender, RoutedEventArgs e)
        {
            var barcodeScanWindow = new BarcodeScanWindow(appContext);
            barcodeScanWindow.ShowDialog();
            if (!barcodeScanWindow.Success) return;
            if (InventoryItem.EANs.Contains(barcodeScanWindow.Barcode)) return;
            InventoryItem.EANs.Add(barcodeScanWindow.Barcode);
            eanContentBox.GetBindingExpression(ListBox.ItemsSourceProperty).UpdateTarget();
        }

        private void eanContentBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (eanContentBox.SelectedItem == null) return;
                InventoryItem.EANs.Remove((string)eanContentBox.SelectedItem);
            }
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            LastFocused = DateTime.Now;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!MarkedForUpload)
            {
                var result = MessageBox.Show(this,
                    "Are you sure want to discard your changes (will be overwritten by the next sync)?", "Warning",
                    MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            InventoryItems.Release(InventoryItem);
        }
    }
}
