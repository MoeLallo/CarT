using System;
using CarCommunication;
using System.Text.Json;


class Program
{
    static void Main(string[] args)
    {

        Console.WriteLine("Hello, World!");

        BluetoothConnector connector = new BluetoothConnector();

        Connect(connector);
        String parameters = JsonCreator();
        connector.SendParameters(parameters);



    }
    static void Connect(BluetoothConnector connector)
    {
        Console.WriteLine("Do you want to connect to the car? only option is yes");
        string connectToCarConfirmation = Console.ReadLine();

        if (connectToCarConfirmation.ToLower() == "yes")
        {
            Console.WriteLine("Connecting...");

            string connectionStatus = connector.ConnectToBluetooth();

            Console.WriteLine(connectionStatus);
        }
    }
    static string JsonCreator()
    {
        var parameters = new
        {
            mode = "pattern",
            timerInerval = "500",
            carSpeed = "2500",
            duration = "2",
            direction = "w",
            ready = "true"

        };
        string JsonString = JsonSerializer.Serialize(parameters);
        return JsonString;
        
    }

}
