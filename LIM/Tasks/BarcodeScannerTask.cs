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
    public class BarcodeScannerTask : IHostedService, IDisposable
    {
        private Timer _timer;

        public const int RUN_EVERY_SECONDS = 10;

        private bool isRunning = false;

        private DateTime nextRun;

        public BarcodeScannerTask()
        {
            nextRun = DateTime.Now;

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
                //if (Debugger.IsAttached)
                //{
                //    throw ex;
                //}
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
            
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //logger.LogInformation("CleanUpTask is stopped");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
    }

}
