using CarCommunication;
using System.Diagnostics;

namespace TestEnvironment
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestBluetoothScan()
        {
            Debug.WriteLine("My output");
            BluetoothConnector connector = new BluetoothConnector();
        }
    }
}