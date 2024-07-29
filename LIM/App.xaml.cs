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
using LIM.Engines;
using LIM.Windows.Helpers;

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

            var inventoryItemEntityManager = new EntityManager<InventoryItem>(GlobalState.UserSettings.ListName, GlobalState.InventoryItemFileName);
            inventoryItemEntityManager.TryLoad();
            GlobalState.SyncListGraphTask = new SyncListGraphTask(graphService, inventoryItemEntityManager);
            GlobalState.SyncListGraphTask.StartAsync(new CancellationToken());

            GlobalState.LimAppContext = new LimAppContext()
            {
                BarcodeScannerService = new BarcodeScanner.BarcodeScannerService(GlobalState.UserSettings),
                WindowManager = new WindowManager(),
                InventoryItems = inventoryItemEntityManager,
                AppStateEngine = new AppStateManager()
            };

            GlobalState.AppLogicLinker = new AppLogicLinker(GlobalState.LimAppContext);

            GlobalState.LimAppContext.BarcodeScannerService.ReStart();
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
