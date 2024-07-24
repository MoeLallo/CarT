
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Threading.Tasks;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace Communicate_With_ESP32_via_Bluetooth_Serial
{
    class MainClass
    {

        private static SerialPort port;
        private static ConcurrentQueue<string> dataQueue = new ConcurrentQueue<string>();
        private static MainClass instance;

        private static bool isConnected = false;
        private static string command = "";
        private static int motorSpeed = 0;
        private static int length = 0;
        private static string direction = "";
        private static string startCommand = "";
        private static DateTime lastSentSpeed = DateTime.MinValue;
        enum CarStates { InitialWaiting, CommandWaiting, Initialization, WaitForRun, Running, Finishing }
        static CarStates carState = CarStates.InitialWaiting;


        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to ESP32 Car Control Program");

            while (true)
            {
                switch (carState)
                {
                    case CarStates.InitialWaiting:
                        HandleInitialWaiting();
                        break;
                    case CarStates.CommandWaiting:
                        HandleCommandWaiting();
                        break;
                    case CarStates.Initialization:
                        HandleInitialization();
                        break;
                    case CarStates.WaitForRun:
                        HandleWaitForRun();
                        break;
                    case CarStates.Running:
                        HandleRunning();
                        break;
                    case CarStates.Finishing:
                        HandleFinishing();
                        break;
                }

            }



            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            static void HandleInitialWaiting()
            {
                ConnectToBluetooth();

                port.WriteLine("Hello from Console.");


                if (isConnected = true)
                {

                    carState = CarStates.CommandWaiting;
                }
            }

            static void HandleCommandWaiting()
            {
                Console.WriteLine("Enter command (Manual run / Tape run):");
                command = Console.ReadLine();
                port.WriteLine(command);
                port.WriteLine("Command sent!");
                carState = CarStates.Initialization;
            }

            static void HandleInitialization()
            {
                if (command == "Manual run")
                {
                    CarMove();

                    carState = CarStates.WaitForRun;

                }
                else if (command == "Tape run")
                {
                    // Add tracking car option later
                    carState = CarStates.WaitForRun;

                }



            }

            static void HandleWaitForRun()
            {
                Console.WriteLine("Type 'run' to start:");

                startCommand = Console.ReadLine();

                if (startCommand.ToLower() == "run")
                {
                    port.WriteLine(startCommand);
                    port.WriteLine("Run!");
                    carState = CarStates.Running;
                }
            }

            static void HandleRunning()
            {
                port.WriteLine("Running!");
                //port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                carState = CarStates.Finishing;


            }
            static void HandleFinishing()
            {
                Console.WriteLine("Car operation finished.");
                carState = CarStates.CommandWaiting;
            }
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            static void BatteryReading()
            {
                while (true) // Continuously check for input and read data
                {
                    Console.WriteLine("Please write 'yes' if you want to check battery level");

                    string userInput = Console.ReadLine();

                    if (userInput == "yes")
                    {
                        string message1 = port.ReadLine();
                        Console.WriteLine(message1);
                        dataQueue.Enqueue(message1);
                        string message2 = port.ReadLine();
                        Console.WriteLine(message2);
                        dataQueue.Enqueue(message2);
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Type 'yes' to read data from the ESP32.");
                    }
                }
            }
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //static void CarMove()
            //{
            //    while (true)
            //    {
            //        Console.WriteLine("Please Input your desired Speed (1000-4095)");
            //        int motorSpeed = int.Parse(Console.ReadLine());

            //        Console.WriteLine("Please Input the duration of the experiment in seconds");
            //        int length = int.Parse(Console.ReadLine());

            //        Console.WriteLine("Please Input the direction (w/a/s/d) or (q) to quit");
            //        string direction = Console.ReadLine();

            //        if (direction == "q") break;

            //        string fullCommand = $"{motorSpeed} {length} {command} run";
            //        port.WriteLine(fullCommand);
            //        dataQueue.Enqueue(fullCommand);
            //    }
            //    port.Close();
            //}
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            static void CarMove()
            {
                Console.WriteLine("Please Input your desired Speed (1000-4095)");
                int motorSpeed = int.Parse(Console.ReadLine());

                Console.WriteLine("Please Input the duration of the experiment in seconds");
                int length = int.Parse(Console.ReadLine());

                Console.WriteLine("Please Input the direction (w/a/s/d) or (q) to quit");
                string direction = Console.ReadLine();

                if (direction == "q") return;

                port.Write($"{motorSpeed} {length} {direction} ");
            }
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


            static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
            {
                SerialPort sp = (SerialPort)sender;
                string indata = sp.ReadExisting();
                Console.WriteLine("Received: " + indata);

                if (indata.Contains("Bluetooth Connected!"))
                {
                    isConnected = true;
                }
                // Console.WriteLine("Data Received:");
                //Console.WriteLine(indata);

                // Store incoming data in a ConcurrentQueue

                //instance.dataQueue.Enqueue(indata);
            }
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            static void SaveDataToCSV(string filePath)
            {
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    while (dataQueue.TryDequeue(out string result))
                    {
                        sw.WriteLine(result);
                    }
                }
            }
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            static List<string> GetBluetoothDevices()  // Method to get all available Bluetooth devices, can later be used 
            {
                BluetoothClient client = new BluetoothClient();
                IReadOnlyCollection<BluetoothDeviceInfo> devices = client.DiscoverDevices();

                List<string> result = new List<string>();

                foreach (BluetoothDeviceInfo device in devices)
                {
                    result.Add($"{device.DeviceName} : {device.DeviceAddress}"); // Choose your device name and pass the MAC address
                }
                return result;
            }
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            static void ConnectToBluetooth()
            {

                // just to get the PortName
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

                if (bluetoothPort == null)
                {
                    Console.WriteLine("No Bluetooth serial port found.");
                    return;
                }

                if (port != null && port.IsOpen)
                {
                    port.Close();
                }


                Console.WriteLine($"Bluetooth serial port found: {bluetoothPort}");
                port = new SerialPort
                {
                    BaudRate = 115200,
                    PortName = bluetoothPort,
                    ReadTimeout = 2000
                };

                port.Open();
                // send and receive confirmation message for debugging
                port.WriteLine("Hello from Console.");
                port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            }
        }
    }
}
