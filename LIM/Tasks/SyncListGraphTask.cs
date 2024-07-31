using LIM.Engines;
using LIM.EntityServices;
using LIM.EntityServices.Helpers;
using LIM.Helpers;
using LIM.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIM.Tasks
{
    public class SyncListGraphTask : IHostedService, IDisposable
    {
        private Timer _timer;

        public const int RUN_EVERY_SECONDS = 10;

        public IServiceProvider Services { get; }
        public IConfiguration Configuration { get; }
        public ListGraphService GraphService { get; }
        public EntityManager<InventoryItem> InventoryItemEntityManger { get; }
        public LimAppContext AppContext { get; }
        public AppStateManager AppStateManager { get; }
        public LimSettings Settings { get; }

        private bool isRunning = false;

        private DateTime nextRun;

        private ILogger Logger = LoggerService.DefaultFactory.CreateLogger<SyncListGraphTask>();

        private bool UpdateColumnInfo { get; set; } = true;

        public DateTime LastSuccessfulUpdate { get; private set; }

        public SyncListGraphTask(ListGraphService graphService, 
            EntityManager<InventoryItem> inventoryItemEntityManger,
            AppStateManager appStateManager,
            LimSettings settings)
        {
            nextRun = DateTime.Now;
            GraphService = graphService;
            InventoryItemEntityManger = inventoryItemEntityManger;
            AppStateManager = appStateManager;
            Settings = settings;
        }

        public void Dispose()
        {
            _timer?.Change(Timeout.Infinite, 0);
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(tryDoWork, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(10));

            //logger.LogInformation("CleanUpTask is started.");

            return Task.CompletedTask;
        }

        private void tryDoWork(object? state)
        {
            try
            {
                if (isRunning) return;
                isRunning = true;
                doWork(state);
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                {
                    throw ex;
                }
                Logger.LogError(ex, "Task Exception");
    }
            isRunning = false;
        }

        private void doWork(object? state)
        {
            if (DateTime.Now > nextRun)
            {
                Sync();
                nextRun = DateTime.Now.AddSeconds(RUN_EVERY_SECONDS);
            }
        }

        private void Sync()
        {
            if(string.IsNullOrWhiteSpace(GraphService.SiteId))
            {
                Logger.LogError("Missing Site Id. Not syncing");
                return;
            }
            if (InventoryItemEntityManger.GetLocalyChangedEntries().Count > 0)
            {
                InventoryItemEntityManger.Save();
            }

            if(UpdateColumnInfo)
            {
                var taskUpdateColumnInfo = GraphService.UpdateColumnInfo(InventoryItemEntityManger);
                taskUpdateColumnInfo.Wait();
                UpdateColumnInfo = false;
            }

            var taskUpload = GraphService.UploadLocalChanges(InventoryItemEntityManger);
            taskUpload.Wait();
            if(taskUpload.Result > 0) Logger.LogInformation($"Uploaded {taskUpload.Result}");

            var taskUpload2 = GraphService.UploadNewItems(InventoryItemEntityManger);
            taskUpload2.Wait();
            if (taskUpload2.Result > 0) Logger.LogInformation($"Created {taskUpload2.Result}");

            var taskLogEntries = CalculateChangesAndCreateLogEntries();
            taskLogEntries.Wait();

            var taskSync = GraphService.GetOrUpdateManager(InventoryItemEntityManger);
            taskSync.Wait();
            if(InventoryItemEntityManger.Changed) InventoryItemEntityManger.Save();
            LastSuccessfulUpdate = DateTime.Now;
            Logger.LogInformation($"Microsoft Graph Sharepoint List Sync Task finished successfully @ {LastSuccessfulUpdate}");
        }

        private async Task CalculateChangesAndCreateLogEntries()
        {
            var changedItems = InventoryItemEntityManger.GetEntriesThatRequireLogEntries();

            foreach (var changedItem in changedItems)
            {
                if (!changedItem.OriginalInventoryFromRemote.HasValue || 
                    changedItem.ActualInventory == changedItem.OriginalInventoryFromRemote) continue;
                if (string.IsNullOrEmpty(changedItem.Id) || changedItem.Id == IEntity.NEW_ID_STR) continue;
                var inventoryLogItem = new InventoryLogItem()
                {
                    StockChange = changedItem.ActualInventory - changedItem.OriginalInventoryFromRemote.Value,
                    InventoryItemDescription = changedItem.Description,
                    InventoryItemId = changedItem.Id,
                    Timestamp = DateTime.UtcNow,
                    Username = AppStateManager.ActiveUser
                };
                var taskCreateLogEntry = await GraphService.CreateNewItem(Settings.LogListName, inventoryLogItem);
                if (!taskCreateLogEntry) Logger.LogError($"Failed to craete log entry for {changedItem.Id} {changedItem.Description}");
                InventoryItemEntityManger.SetRequiresLogEntry(changedItem, false);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //logger.LogInformation("CleanUpTask is stopped");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
    }

}
