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
using System.Windows.Threading;

namespace LIM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public LimAppContext LimAppContext { get; private set; }

        DispatcherTimer Timer = new DispatcherTimer();
        public MainWindow()
        {
            LimAppContext = GlobalState.LimAppContext;
            InitializeComponent();
            MainGrid.DataContext = LimAppContext.AppStateEngine;
            Timer.Tick += Timer_Tick;
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            UpdateSyncStatus();
            UpdateBarcodeStatus();
        }

        private void UpdateBarcodeStatus()
        {
            BarcodeScannerLabel.Content =
                (GlobalState.LimAppContext.BarcodeScannerService.IsConnected ? $"Connected" : "Disconnected") +
                $" @ {GlobalState.LimAppContext.BarcodeScannerService.ComPort}";

            ReconnectButton.Visibility = GlobalState.LimAppContext.BarcodeScannerService.IsConnected ? Visibility.Hidden : Visibility.Visible;
        }

        private void UpdateSyncStatus()
        {
            var lastUpdate = GlobalState.SyncListGraphTask.LastSuccessfulUpdate;
            if (lastUpdate == null || lastUpdate == DateTime.MinValue) return;
            var diffSinceNow = DateTime.Now - lastUpdate;
            if (diffSinceNow.TotalMinutes < 1)
            {
                SyncStatusLabel.Content = $"{diffSinceNow.TotalSeconds:N0} seconds ago";
            }
            else if (diffSinceNow.TotalMinutes < 5)
            {
                SyncStatusLabel.Content = $"{diffSinceNow.TotalMinutes:N0} minutes ago";
            }
            else
            {
                SyncStatusLabel.Content = lastUpdate.ToString("dd.MM.yyyy HH:mm:ss");
            }
        }

        public EntityManager<InventoryItem> inventoryItemEntityManager { get; private set; }
        public IConfigurationRoot AppConfig { get; private set; }

        private void InventoryList_Click(object sender, RoutedEventArgs e)
        {
            var inventoryItemListWindow = new InventoryItemListWindow(GlobalState.LimAppContext);
            inventoryItemListWindow.Show();
        }

        private void Button_Click_Settings(object sender, RoutedEventArgs e)
        {
            var LimSettingsWindow = new LimSettingsWindow();
            LimSettingsWindow.ShowDialog();
        }

        private void Button_Click_Reconnect(object sender, RoutedEventArgs e)
        {
            ReconnectButton.Visibility = Visibility.Hidden;
            GlobalState.LimAppContext.BarcodeScannerService.ReStart();
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var barcodeWindow = new BarcodeScanWindow(GlobalState.LimAppContext, true);
            barcodeWindow.Show();
        }
    }
}