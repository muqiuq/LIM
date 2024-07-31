using LIM.EntityServices;
using LIM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace LIM.Windows.Helpers
{
    public class WindowManager
    {

        Dictionary<InventoryItem, InventoryItemWindow> InventoryItemToWindow = new Dictionary<InventoryItem, InventoryItemWindow>();
        private object _lock = new object();

        private void CleanUpClosedWindows() 
        {

            foreach (var itemKeyVal in InventoryItemToWindow)
            {
                if (!itemKeyVal.Value.IsVisible)
                {
                    InventoryItemToWindow.Remove(itemKeyVal.Key);
                }
            }
        }

        public void OpenOrFocusInventoryItemWindow(InventoryItem item, LimAppContext appContext, int addOrRemove = 0, bool closeOthers = false) 
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke((Action)delegate {
                    OpenOrFocusInventoryItemWindow(item, appContext, addOrRemove, closeOthers);
                });
                return;
            }
            
            lock (_lock)
            {
                if (closeOthers)
                {
                    CloseAllWindowsExcept(item);
                }
                CleanUpClosedWindows();
                if(!InventoryItemToWindow.ContainsKey(item) )
                {
                    var wind = new InventoryItemWindow(item, appContext);
                    InventoryItemToWindow.Add(item, wind);
                    if(!wind.HasBeenClosed) wind.Show();
                    if (addOrRemove != 0) wind.AddOrRemove(addOrRemove);
                }
                else
                {
                    var wind = InventoryItemToWindow[item];
                    wind.Activate();
                    if (addOrRemove != 0) wind.AddOrRemove(addOrRemove);
                }
            }
            
        }

        private void CloseAllWindowsExcept(params InventoryItem[] items)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke((Action)delegate { CloseAllWindowsExcept(items); });
                return;
            }

            // ReSharper disable once InconsistentlySynchronizedField
            InventoryItemToWindow.Where(x => !items.Contains(x.Key))
                .Select(x => x.Value).ToList().ForEach(x => x.CloseAndUpload());
        }

        public void CloseLastActiveWindow()
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke((Action)delegate { CloseLastActiveWindow(); });
                return;
            }

            lock (_lock)
            {
                CleanUpClosedWindows();

                InventoryItemToWindow
                    .OrderByDescending(x => x.Value.LastFocused).Select(x => x.Value)
                    .FirstOrDefault()?.CloseAndUpload();
            }
        }
    }
}
