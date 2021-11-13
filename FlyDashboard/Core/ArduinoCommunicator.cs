using System;
using System.IO.Ports;
using System.Linq;

namespace FlyDashboard.Core
{
    public class CommandEventArgs : EventArgs
    {
        public string commandID;
        public string commandValue;
    }

    public class ArduinoCommunicator
    {

        private SerialPort? _port;
        public bool IsConnected { get => _port != null && _port.IsOpen; }

        public event EventHandler<CommandEventArgs>? OnCommandRequest;

        public ArduinoCommunicator()
        {
        }

        public bool Connect(string port)
        {
            if(DoesPortExists(port))
            {
                _port = new SerialPort(port, 9600, Parity.None, 8, StopBits.One);

                try
                {
                    _port.Open();
                }
                catch(Exception ex)
                {
                    System.Console.WriteLine("Cannot open Port: " + port + " is close.");
                }

                _port.DataReceived += SerialPort_DataReceived;
                _port.ErrorReceived += SerialPort_ErrorReceived;

                SendConnectionCommand();

                GenericEvents.RaiseConnectDashboard(port);
                return true;
            }

            return false;
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            System.Console.WriteLine("error");
            Disconnect();
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while(_port.BytesToRead > 0)
            {
                string readLine = _port.ReadLine();
                System.Console.WriteLine(readLine);

                string[] cmdData = readLine.Split("-");

                CommandEventArgs args = new CommandEventArgs();
                args.commandID = cmdData[0];
                args.commandValue = cmdData[1];

                OnCommandRequest?.Invoke(this, args);

                //m_oSimConnect.SetDataOnSimObject(m_oSelectedSimvarRequest.eDef, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, dValue);

            }
        }

        public void Disconnect()
        {
            if(IsConnected)
            {
                SendDisconnectionCommand();

                _port.DataReceived -= SerialPort_DataReceived;
                _port.ErrorReceived -= SerialPort_ErrorReceived;


                _port.Close();
                _port.Dispose();

                GenericEvents.RaiseDisconnectDashboard();

                _port = null;
            }
        }

        public void SendConnectionCommand()
        {
            SendCommand("CONNE");
        }

        public void SendDisconnectionCommand()
        {
            SendCommand("DISCO");
        }

        public void SendAltitudeCommand(int altitude)
        {
            const string cmd = "ALTIT";
            SendCommand(cmd + altitude.ToString());
        }

        public void SendHeadingCommand(int hdg)
        {
            const string cmd = "HEADI";
            SendCommand(cmd + hdg.ToString());
        }

        public void SendGroundSpeedCommand(int speed)
        {
            const string cmd = "GROSP";
            SendCommand(cmd + speed.ToString());
        }

        public void SendAirSpeedCommand(int speed)
        {
            const string cmd = "AIRSP";
            SendCommand(cmd + speed.ToString());
        }

        private void SendCommand(string cmd)
        {
            if(IsConnected)
            {
                _port.Write("$");
                _port.Write(cmd);
                _port.Write("#");
            }
        }

        private bool DoesPortExists(string port)
        {
            return SerialPort.GetPortNames().ToList().Contains(port);
        }
    }
}
