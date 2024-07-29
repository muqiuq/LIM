using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LIM.BarcodeScanner;
using LIM.Engines;
using LIM.EntityServices;
using LIM.Models;
using LIM.Windows.Helpers;

namespace LIM
{
    public class LimAppContext
    {

        public BarcodeScannerService BarcodeScannerService { get; set; }

        public WindowManager WindowManager { get; set; }

        public EntityManager<InventoryItem> InventoryItems { get; set; }

        public AppStateManager AppStateEngine { get; set; } 

    }
}
