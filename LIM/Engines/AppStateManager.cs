using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LIM.BarcodeScanner;
using LIM.Windows.Helpers;

namespace LIM.Engines
{
    public class AppStateManager : INotifyPropertyChanged
    {
        public AppState State { get; set; } = AppState.Lookup;

        public string ActiveUser { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public IEnumerable<AppState> AppStates => Enum.GetValues(typeof(AppState)).Cast<AppState>();

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handle = PropertyChanged;
            handle?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public delegate void OnSingleScanBarcodeDelegate(string barcode);

        public event OnSingleScanBarcodeDelegate OnSingleScanBarcode;

        public void FireOnSingleScanBarcode(string barcode)
        {
            OnSingleScanBarcode?.Invoke(barcode);
        }
    }
}