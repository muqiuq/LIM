using LIM.EntityServices;
using LIM.Helpers;
using LIM.Models;
using LIM.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace LIM
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            GlobalState.AppConfig = GetAppConfig();
            GlobalState.UserSettings = LimSettings.TryLoadFromFileOrNew();
            var graphService = new ListGraphService(GlobalState.AppConfig, GlobalState.UserSettings);

            GlobalState.InventoryItems = new EntityManager<InventoryItem>(GlobalState.UserSettings.ListName, GlobalState.InventoryItemFileName);
            GlobalState.InventoryItems.TryLoad();
            GlobalState.SyncListGraphTask = new SyncListGraphTask(graphService, GlobalState.InventoryItems);
            GlobalState.SyncListGraphTask.StartAsync(new CancellationToken());
            GlobalState.BarcodeScannerService = new BarcodeScanner.BarcodeScannerService(GlobalState.UserSettings);
            GlobalState.BarcodeScannerService.ReStart();
            base.OnStartup(e);
        }

        private IConfigurationRoot GetAppConfig()
        {
            var appConfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddUserSecrets<MainWindow>()
                .Build();

            return appConfig;
        }
    }

}
