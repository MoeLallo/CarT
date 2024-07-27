using System;
using CarCommunication;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        using (var connector = new BluetoothConnector())
        {
            
            string connectionStatus = connector.ConnectToBluetooth();
            Console.WriteLine("Connection Status: " + connectionStatus);

            
            if (connectionStatus.Contains("Connected"))
            {
                // Send parameters using Json file
                var parameters = new
                {
                    mode = "pattern",
                    carSpeed = 1500,
                    duration = 5,
                    timeInterval = 1,
                    ready = true
                };
                string jsonString = System.Text.Json.JsonSerializer.Serialize(parameters);
                string parameterStatus = connector.SendParameters(jsonString);
                Console.WriteLine("Parameter Status: " + parameterStatus);

                
                System.Threading.Thread.Sleep(1000);

                
                string runStatus = connector.SendRunMsg();
                Console.WriteLine("waiting for run state: car speed is " + runStatus);



                var cancellationTokenSource = new CancellationTokenSource();
                var token = cancellationTokenSource.Token;
                var handleRunningTask = connector.HandleRunningAsync(parameters.timeInterval, parameters.duration, token);
                await Task.Delay(TimeSpan.FromSeconds(parameters.duration));    // adding the time interval delay is very important
                cancellationTokenSource.Cancel();

                try
                {
                    await handleRunningTask;
                }
                catch (OperationCanceledException)
                {
                    // Task was cancelled
                }
            }
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}