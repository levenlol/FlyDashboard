using System;
using System.ComponentModel;

namespace FlyDashboard.Core
{
    public class ConnectDashboardArgs : EventArgs
    {
        public string port = "";
    }
    public static class GenericEvents
    {
        public static event EventHandler? OnReconnectSim;
        public static void RaiseReconnectSim()
        {
            OnReconnectSim?.Invoke(typeof(GenericEvents), EventArgs.Empty);
        }

        public static event EventHandler? OnConnectDashboard;
        public static void RaiseConnectDashboard(string port)
        {
            ConnectDashboardArgs args = new ConnectDashboardArgs();
            args.port = port;

            OnConnectDashboard?.Invoke(typeof(GenericEvents), args);
        }

        public static event EventHandler? OnDisconnectDashboard;
        public static void RaiseDisconnectDashboard()
        {
            OnDisconnectDashboard?.Invoke(typeof(GenericEvents), EventArgs.Empty);
        }
    }
}
