using LIM.EntityServices;
using LIM.Models;
using LIM.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LIM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            inventoryItemEntityManager = GlobalState.InventoryItems;
        }

        public EntityManager<InventoryItem> inventoryItemEntityManager { get; private set; }
        public IConfigurationRoot AppConfig { get; private set; }

        private void InventoryList_Click(object sender, RoutedEventArgs e)
        {
            var inventoryItemListWindow = new InventoryItemListWindow(inventoryItemEntityManager);
            inventoryItemListWindow.Show();
        }

        private void Button_Click_Settings(object sender, RoutedEventArgs e)
        {
            var LimSettingsWindow = new LimSettingsWindow();
            LimSettingsWindow.ShowDialog();
        }
    }
}