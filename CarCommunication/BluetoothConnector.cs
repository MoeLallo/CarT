using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Management;
using InTheHand.Net.Sockets; 
using InTheHand.Net.Bluetooth;

namespace CarCommunication
{
    public class BluetoothConnector : IDisposable
    {
        private SerialPort port;
        private bool disposed = false;


        public  String ConnectToBluetooth()
        {
            if (port == null || !port.IsOpen)
            {
                try
                {
                    // Hard coded for now, will be fixed later to get teh PortName Automatically
                    port = new SerialPort
                    {

                        BaudRate = 115200,
                        PortName = "COM9",
                        ReadTimeout = 1000
                    };
                    port.Open();
                    port.Write("Hello.");

                    String bluetoothConfirmationMsg = port.ReadLine();
                    return bluetoothConfirmationMsg;        // send additional information for the GUI (Cart Type, Serial port, Cart ID)
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            }
            else
            {
                return "Port is Already open.";
            }
        }
        public String SendParameters(string parameters)
        {
            if (port != null && port.IsOpen)
            {
                port.Write(parameters + "."); // Add a period at the end of the message to signal the end of the JSON string
            }
            String parameterConfirmationMsg = port.ReadLine();

            return parameterConfirmationMsg;
        }
        public String SendRunMsg()
        {
            port.WriteLine("run.");
            return port.ReadLine();
        }
        public String HandleRunning()
        {
   
            return port.ReadLine();
        }

        public async Task HandleRunningAsync(int timeInterval, int duration, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    string runningStatus = HandleRunning();
                    Console.WriteLine("Running State: " + runningStatus);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                await Task.Delay(TimeSpan.FromSeconds(timeInterval), token);
            }
        }


        #region Dispose Method
        public void Dispose()
        {
            disposed = true;
            if (port != null)
            {
                port.Close();
                port.Dispose();
            }
        }
       
        #endregion
    }
}
