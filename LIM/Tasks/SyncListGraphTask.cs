using LIM.EntityServices;
using LIM.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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

        private bool isRunning = false;

        private DateTime nextRun;

        public SyncListGraphTask(ListGraphService graphService, EntityManager<InventoryItem> inventoryItemEntityManger)
        {
            nextRun = DateTime.Now;
            GraphService = graphService;
            InventoryItemEntityManger = inventoryItemEntityManger;
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
                Debug.WriteLine(ex);
                //logger.LogError($"CleanUpTask {ex.Message}", ex);
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
                Debug.WriteLine("Missing Site Id. Not syncing");
                return;
            }
            if (InventoryItemEntityManger.GetLocalyChangedEntries().Count > 0)
            {
                InventoryItemEntityManger.Save();
            }
            var taskUpload = GraphService.UploadLocalChanges(InventoryItemEntityManger);
            taskUpload.Wait();

            var taskSync = GraphService.GetOrUpdateManager(InventoryItemEntityManger);
            taskSync.Wait();
            if(InventoryItemEntityManger.Changed) InventoryItemEntityManger.Save();
            Debug.WriteLine("Sync Task runned");
        }

       

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //logger.LogInformation("CleanUpTask is stopped");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
    }

}
