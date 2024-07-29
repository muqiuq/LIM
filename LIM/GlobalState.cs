using LIM.BarcodeScanner;
using LIM.EntityServices;
using LIM.Helpers;
using LIM.Models;
using LIM.Tasks;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LIM.Engines;

namespace LIM
{
    internal static class GlobalState
    {
        public static string InventoryItemFileName = "inventory.json";

        public static IConfigurationRoot AppConfig { get; set; }

        public static SyncListGraphTask SyncListGraphTask { get; internal set; }

        public static LimSettings UserSettings { get; set; }
       
        public static LimAppContext LimAppContext { get; set; }
        public static AppLogicLinker AppLogicLinker { get; set; }
    }
}
