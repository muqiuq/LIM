using LIM.EntityServices;
using LIM.Models;
using LIM.Windows.Helpers;
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

namespace LIM.Windows
{
    /// <summary>
    /// Interaction logic for InventoryItemListWindow.xaml
    /// </summary>
    public partial class InventoryItemListWindow : Window
    {
        public EntityManager<InventoryItem> InventoryItems { get; }

        public InventoryItemListWindow(EntityManager<InventoryItem> inventoryItems)
        {
            InitializeComponent();
            InventoryItems = inventoryItems;
            inventoryItemDataGrid.ItemsSource = InventoryItems;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void inventoryItemDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (inventoryItemDataGrid.SelectedItem != null && inventoryItemDataGrid.SelectedItem is InventoryItem)
            {
                StaticWindowManager.OpenOrFocusInventoryItemWindow((InventoryItem)inventoryItemDataGrid.SelectedItem, InventoryItems);
            }
                
        }
    }
}
