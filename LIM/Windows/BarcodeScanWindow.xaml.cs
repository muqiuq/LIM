using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LIM.Engines;

namespace LIM.Windows
{
    /// <summary>
    /// Interaction logic for BarcodeScanWindow.xaml
    /// </summary>
    public partial class BarcodeScanWindow : Window
    {
        private readonly LimAppContext appContext;
        private readonly AppState beforeState;

        public string Barcode { get; private set; }

        public bool Success { get; private set; } = true;

        public BarcodeScanWindow(LimAppContext appContext)
        {
            this.appContext = appContext;
            InitializeComponent();
            beforeState = appContext.AppStateEngine.State;
            appContext.AppStateEngine.State = AppState.SingleScan;
            appContext.AppStateEngine.OnSingleScanBarcode += AppStateEngine_OnSingleScanBarcode;
        }

        private void AppStateEngine_OnSingleScanBarcode(string barcode)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => AppStateEngine_OnSingleScanBarcode(barcode));
                return;
            }
            BarcodeTextBox.Text = barcode;
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            appContext.AppStateEngine.OnSingleScanBarcode -= AppStateEngine_OnSingleScanBarcode;
            Barcode = BarcodeTextBox.Text;
            appContext.AppStateEngine.State = beforeState;
            if (string.IsNullOrWhiteSpace(Barcode)) Success = false;
        }

        private void CommandBinding_Close(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void CommandBinding_Cancel(object sender, ExecutedRoutedEventArgs e)
        {
            Success = false;
            Close();
        }
    }
}
