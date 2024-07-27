using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Management;
using InTheHand.Net.Sockets; 
using InTheHand.Net.Bluetooth;
using System.Text.Json;


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
                    
                    String devicePortName = GetDevicePortName();
                    port = new SerialPort
                    {
                         
                        BaudRate = 115200,
                        PortName = devicePortName,
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
                port.Write(parameters + "."); // our arduino code will read string until (.)
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
            string updatedParameters = port.ReadLine();
            try
            {
                var doc = JsonDocument.Parse(updatedParameters);

                int carSpeed = doc.RootElement.GetProperty("carSpeed").GetInt32();
                int batteryADC = doc.RootElement.GetProperty("BatteryADC").GetInt32();
                float batteryVoltage = doc.RootElement.GetProperty("BatteryVoltage").GetSingle();

                string formattedOutput = $"Car Speed: {carSpeed}\nBattery ADC: {batteryADC}\nBattery Voltage: {batteryVoltage}";

                return formattedOutput;

            }
            catch (Exception ex) {
                Console.WriteLine("Error parsing JSON: " + ex.Message);
                return "Error parsing JSON";
            }

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
        public String GetDevicePortName()
        {
            string bluetoothPort = null;
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SerialPort"))
            {
                foreach (var BTport in searcher.Get())
                {
                    string portName = BTport.GetPropertyValue("DeviceID")?.ToString();
                    string description = BTport.GetPropertyValue("PNPDeviceID")?.ToString();

                    if (description != null && description.Contains("E831CDC4DF7E"))
                    {
                        bluetoothPort = portName;
                        break;

                    }
                }
            }
            return bluetoothPort;
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
