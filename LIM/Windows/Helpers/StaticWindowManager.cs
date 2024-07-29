using LIM.EntityServices;
using LIM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIM.Windows.Helpers
{
    public static class StaticWindowManager
    {

        static Dictionary<InventoryItem, InventoryItemWindow> InventoryItemToWindow = new Dictionary<InventoryItem, InventoryItemWindow>();
        private static object _lock = new object();

        private static void CleanUpClosedWindows() 
        {

            foreach (var itemKeyVal in InventoryItemToWindow)
            {
                if (!itemKeyVal.Value.IsVisible)
                {
                    InventoryItemToWindow.Remove(itemKeyVal.Key);
                }
            }
        }

        public static void OpenOrFocusInventoryItemWindow(InventoryItem item, EntityManager<InventoryItem> manager) 
        {
            lock (_lock)
            {
                CleanUpClosedWindows();
                if(!InventoryItemToWindow.ContainsKey(item) )
                {
                    var wind = new InventoryItemWindow(item, manager);
                    InventoryItemToWindow.Add(item, wind);
                    wind.Show();
                }
                else
                {
                    var wind = InventoryItemToWindow[item];
                    wind.Activate();
                }
            }
            
        }

    }
}
