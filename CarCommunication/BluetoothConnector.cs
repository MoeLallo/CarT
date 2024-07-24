using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Management;
using InTheHand.Net.Sockets; 
using InTheHand.Net.Bluetooth;

namespace CarCommunication
{
    public class BluetoothConnector 
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
                    port.Close();
                    port.Open();
                    port.Write("Hello.");

                    String bluetoothConfirmationMsg = port.ReadLine();
                    return bluetoothConfirmationMsg;
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
        public void SendParameters(string parameters)
        {
            if (port != null && port.IsOpen)
            {
                port.Write(parameters + "."); // Add a period at the end of the message to signal the end of the JSON string
            }
        }

    }
}
