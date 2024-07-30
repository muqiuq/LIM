using LIM.Helpers;
using Microsoft.Graph.Models.ExternalConnectors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIM.BarcodeScanner
{
    public class BarcodeScannerService : IDisposable
    {

        public delegate void OnBarcodeLineReceivedDelegate(string barcode);
        public event OnBarcodeLineReceivedDelegate OnBarcodeLineReceived;

        public BarcodeScannerService(LimSettings limSettings) {
            LimSettings = limSettings;
        }

        public void ReStart()
        {
            ScannerSerialPort?.Close();

            ScannerSerialPort = new SerialPort(LimSettings.BarcodeScannerComPort, LimSettings.BarcodeScannerBaud);

            try
            {
                ScannerSerialPort.Open();

                ScannerSerialPort.DataReceived += SpOnDataReceived;
            }
            catch (FileNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void Stop()
        {
            ScannerSerialPort?.Close();
        }

        private void SpOnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var line = ScannerSerialPort.ReadExisting().Trim();
            var parts = line.Split("\r");
            foreach (var part in parts)
            {
                ExecuteBarcodeAction(part);
            }
        }

        private void ExecuteBarcodeAction(string part)
        {
            Debug.WriteLine(part);
            OnBarcodeLineReceived?.Invoke(part);
        }


        public LimSettings LimSettings { get; }
        public SerialPort ScannerSerialPort { get; private set; }
        public bool IsConnected => ScannerSerialPort?.IsOpen ?? false;
        public string ComPort => ScannerSerialPort?.PortName ?? "";

        public void Dispose()
        {
            ScannerSerialPort.Dispose();
        }
    }
}
