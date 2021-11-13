﻿using FlyDashboard.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FlyDashboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Stopwatch _stopwatch;
        long _lastElapsedMS = 0;
        public MainWindow()
        {
            InitializeComponent();

            _stopwatch = Stopwatch.StartNew();
            SimConnection.SimConnection.OnDataReceived += SimConnection_OnDataReceived;
        }

        private void SimConnection_OnDataReceived(object? sender, DashboardEventArgs e)
        {
            if(_stopwatch.ElapsedMilliseconds - _lastElapsedMS > 1000)
            {
                if (DashboardInterface.Communicator != null)
                {
                    DashboardInterface.Communicator.SendAltitudeCommand(Convert.ToInt32(e.Info.altitude));
                    DashboardInterface.Communicator.SendGroundSpeedCommand(Convert.ToInt32(e.Info.groundSpeed));
                    DashboardInterface.Communicator.SendAirSpeedCommand(Convert.ToInt32(e.Info.airSpeed));
                    DashboardInterface.Communicator.SendHeadingCommand(Convert.ToInt32(e.Info.heading));
                }

                _lastElapsedMS = _stopwatch.ElapsedMilliseconds;
            }
        }
    }
}