using CarCommunication;

Console.WriteLine("Hello, World!");


using BluetoothConnector connector = new BluetoothConnector();

Console.WriteLine("Do you want to connect to the car? yes/no");
String connectToCarConfirmation = Console.ReadLine();
if (connectToCarConfirmation == "yes")
{
    connector.ConnectToBluetooth();
}

Console.WriteLine("Do you want to continue to your setup? yes/no");
String continueToCommand = Console.ReadLine();
if ( continueToCommand == "yes")
{
    connector.SendCommand();
}

Console.WriteLine("Do you want to continue to your initialization? yes/no");
String continueToInitialization = Console.ReadLine();
if (continueToInitialization == "yes")
{
    connector.Initializing();
}

//connector.WatiforRun();
//connector.Running(); 