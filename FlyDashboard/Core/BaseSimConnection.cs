using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using Microsoft.FlightSimulator.SimConnect;

namespace FlyDashboard.Core
{
    public enum SimConnectionStatus
    {
        Disconnected,
        Tentative,
        Connected
    }

    public abstract class BaseSimConnection : INotifyPropertyChanged
    {
        protected SimConnect? simConnection;

        private Timer timer = new Timer();
        private IntPtr Handle;

        private SimConnectionStatus connectionStatus = SimConnectionStatus.Disconnected;
        public SimConnectionStatus ConnectionStatus
        {
            get { return connectionStatus; }

            private set
            {
                connectionStatus = value;

                // invoke on main thread. this is also used to bind UI elements.
                Application.Current?.Dispatcher.Invoke(new Action(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConnectionStatus"))));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public const int USER_SIMCONNECT = 0x0402;

        // Abstact properties
        public abstract string FunctionalityName { get; }
        protected abstract double PollingInterval { get; }

        public bool IsConnected => ConnectionStatus == SimConnectionStatus.Connected && simConnection != null;

        protected BaseSimConnection()
        {
            //Connect();

            GenericEvents.OnReconnectSim += GenericEvents_OnReconnectSim;
        }

        private void GenericEvents_OnReconnectSim(object? sender, EventArgs e)
        {
            Connect();
        }

        private void Tick(object sender, ElapsedEventArgs e)
        {
            try 
            {
                if (ConnectionStatus == SimConnectionStatus.Tentative || IsConnected)
                {
                    simConnection.ReceiveMessage();
                }
            }
            catch 
            {
                System.Console.WriteLine("Connection lost...");
                Clear();
            }
        }

        ~BaseSimConnection()
        {
            Clear();

        }

        public void Connect()
        {
            if (ConnectionStatus == SimConnectionStatus.Disconnected)
            {
                try
                {
                    Handle = new IntPtr();
                    simConnection = new SimConnect(FunctionalityName, Handle, USER_SIMCONNECT, null, 0);

                    simConnection.OnRecvOpen += new SimConnect.RecvOpenEventHandler(SimConnect_OnRecvOpen);
                    simConnection.OnRecvQuit += new SimConnect.RecvQuitEventHandler(SimConnect_OnRecvQuit);

                    // Listen to exceptions
                    simConnection.OnRecvException += new SimConnect.RecvExceptionEventHandler(SimConnect_OnRecvException);

                    // Catch a simobject data request
                    simConnection.OnRecvSimobjectData += new SimConnect.RecvSimobjectDataEventHandler(SimConnect_OnRecvObjectData);
                    simConnection.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(SimConnect_OnRecvSimobjectDataBytype);

                    ConnectionStatus = SimConnectionStatus.Tentative;

                    SetupSimData();
                    TryStartTimer();
                }
                catch
                {
                    ConnectionStatus = SimConnectionStatus.Disconnected;
                }
            }
        }

        protected abstract void SetupSimData();

        protected virtual void OnRecvObjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data) { }
        private void SimConnect_OnRecvObjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            // invoke on main thread.
            Application.Current?.Dispatcher.Invoke(new Action(() => OnRecvObjectData(sender, data)));
        }

        private void SimConnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            SIMCONNECT_EXCEPTION eException = (SIMCONNECT_EXCEPTION)data.dwException;
            Console.WriteLine("SimConnect_OnRecvException: " + eException.ToString());

            ConnectionStatus = SimConnectionStatus.Disconnected;
            Clear();
        }

        private void SimConnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            // didn't find any difference between SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE and SIMCONNECT_RECV_SIMOBJECT_DATA. should be safe to do that.
            // invoke on main thread.
            Application.Current?.Dispatcher.Invoke(new Action(() => OnRecvObjectData(sender, data)));
        }

        private void TryStartTimer()
        {
            if (PollingInterval > 0)
            {
                timer.Interval = PollingInterval;
                timer.Elapsed += Tick;

                timer.Start();
            }
        }

        protected virtual void SimConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Console.WriteLine("SimConnect Connected.");

            ConnectionStatus = SimConnectionStatus.Connected;
        }

        protected virtual void SimConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Console.WriteLine("SimConnect Disconnected.");

            timer.Stop();

            Clear();
        }

        private void Clear()
        {
            if (simConnection != null)
            {
                simConnection = null;
            }

            timer.Stop();

            ConnectionStatus = SimConnectionStatus.Disconnected;
            Handle = IntPtr.Zero;
        }
    }
}
