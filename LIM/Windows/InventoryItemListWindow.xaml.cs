using LIM.EntityServices;
using LIM.EntityServices.Helpers;
using LIM.Models;
using LIM.Windows.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private readonly CollectionViewSource itemSourceList;
        private readonly ICollectionView itemSourceListView;
        public LimAppContext AppContext { get; }
        public EntityManager<InventoryItem> InventoryItems { get; }

        public InventoryItemListWindow(LimAppContext appContext)
        {
            AppContext = appContext;
            InitializeComponent();
            InventoryItems = AppContext.InventoryItems;
            

            itemSourceList = new CollectionViewSource() { Source = InventoryItems };
            itemSourceListView = itemSourceList.View;
            inventoryItemDataGrid.ItemsSource = itemSourceList.View;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var itemFilter = new Predicate<object>(TargetItemFilter);

            itemSourceListView.Filter = itemFilter;
        }

        private bool TargetItemFilter(object obj)
        {
            var searchTerm = searchTexBox.Text.ToLower();
            var p = (InventoryItem)obj;

            if (string.IsNullOrWhiteSpace(searchTerm)) return true;
            searchTerm = searchTerm.Trim();

            return
                p.Id.ToString().Contains(searchTerm)
                || p.Description.ToLower().Contains(searchTerm)
                || string.Join("", p.EANs).Contains(searchTerm);
        }

        private void inventoryItemDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedItem();
        }

        private void OpenSelectedItem()
        {
            if (inventoryItemDataGrid.SelectedItem != null && inventoryItemDataGrid.SelectedItem is InventoryItem)
            {
                AppContext.WindowManager.OpenOrFocusInventoryItemWindow((InventoryItem)inventoryItemDataGrid.SelectedItem, AppContext);
            }
        }

        private void CommandBinding_Search(object sender, ExecutedRoutedEventArgs e)
        {
            searchTexBox.Focus();
        }

        private void searchTexBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !itemSourceListView.IsEmpty)
            {
                inventoryItemDataGrid.SelectedIndex = 0;
                inventoryItemDataGrid.Focus();
            }
        }

        private void inventoryItemDataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OpenSelectedItem();
            }
        }

        private void Button_Click_New(object sender, RoutedEventArgs e)
        {
            AppContext.WindowManager.OpenOrFocusInventoryItemWindow(new InventoryItem() { Id=IEntity.NEW_ID_STR }, AppContext);
        }
    }
}
