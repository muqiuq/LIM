﻿using LIM.Helpers;
using LIM.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIM.Engines
{
    public class AppLogicLinker
    {
        public LimAppContext AppContext { get; }

        public AppStateManager StateManager { get; }

        public ILogger Logger = LoggerService.DefaultFactory.CreateLogger<AppLogicLinker>();    

        public AppLogicLinker(LimAppContext appContext)
        {
            AppContext = appContext;
            StateManager = appContext.AppStateEngine;
            AppContext.BarcodeScannerService.OnBarcodeLineReceived += BarcodeScannerService_OnBarcodeLineReceived;
        }

        private void BarcodeScannerService_OnBarcodeLineReceived(string barcode)
        {
            if (StateManager.State == AppState.SingleScan)
            {
                StateManager.FireOnSingleScanBarcode(barcode);
            }

            else if (barcode.StartsWith("STATE:"))
            {
                var stateString = barcode.Split(":").Last().Trim();
                if (Enum.TryParse(stateString, true, out AppState state))
                {
                     StateManager.State = state;
                    Logger.LogDebug($"Change state to {stateString}");
                }
            }

            else if (barcode.StartsWith("USER:"))
            {
                var userString = barcode.Split(":", 2).Last().Trim();
                Logger.LogInformation($"Active user set to {userString}");
                StateManager.ActiveUser = userString;
            }

            else if (barcode.StartsWith("CLOSE"))
            {
                AppContext.WindowManager.CloseLastActiveWindow();
            }

            else if (StateManager.State == AppState.Lookup 
                || StateManager.State == AppState.CheckIn 
                || StateManager.State == AppState.CheckOut)
            {
                var inventoryItems = AppContext.InventoryItems
                    .Where(i => i.Value.EANs.Contains(barcode))
                    .Select(x => x.Value).ToList();

                var amount = 0;
                if (StateManager.State == AppState.CheckIn) amount = 1;
                if (StateManager.State == AppState.CheckOut) amount = -1;

                if (inventoryItems.Any())
                {
                    foreach (var inventoryItem in inventoryItems)
                    {
                        AppContext.WindowManager.OpenOrFocusInventoryItemWindow(inventoryItem, AppContext,
                            addOrRemove: amount,
                            closeOthers: StateManager.State is AppState.CheckIn or AppState.CheckOut);
                    }
                }
                else
                {
                    AppContext.WindowManager.OpenSelectInventoryItemWindow(AppContext, barcode);
                }
            }
        }
    }
}
