using FlyDashboard.Core;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FlyDashboard.UI
{
    /// <summary>
    /// Interaction logic for SimConnectionControl.xaml
    /// </summary>
    public partial class SimConnectionControl : UserControl, INotifyPropertyChanged
    {
        private DashboardSimConnection _simConnection;

        #region LedColor
        private Brush _ledColor;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Brush LedColor
        {
            get
            {
                return _ledColor;
            }

            set
            {
                _ledColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LedColor"));
            }
        }
        #endregion

        public DashboardSimConnection SimConnection { get => _simConnection; }

        public SimConnectionControl()
        {
            InitializeComponent();

            DataContext = this;
            LedColor = new SolidColorBrush(Colors.Red);

            _simConnection = new DashboardSimConnection();
            _simConnection.OnDataReceived += SimConnection_OnDataReceived;
            _simConnection.PropertyChanged += SimConnection_PropertyChanged;
        }

        private void SimConnection_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(_simConnection.ConnectionStatus))
            {
                if(_simConnection.ConnectionStatus == SimConnectionStatus.Disconnected)
                {
                    LedColor = new SolidColorBrush(Colors.Red);
                }
                else if (_simConnection.ConnectionStatus == SimConnectionStatus.Connected)
                {
                    LedColor = new SolidColorBrush(Colors.Green);
                }
                else if (_simConnection.ConnectionStatus == SimConnectionStatus.Tentative)
                {
                    LedColor = new SolidColorBrush(Colors.Yellow);
                }
            }
        }

        private void SimConnection_OnDataReceived(object? sender, DashboardEventArgs e)
        {
            AltitudeTextBlock.Text = e.Info.altitude.ToString();
        }

        private void ReconnectButton_Click(object sender, RoutedEventArgs e)
        {
            GenericEvents.RaiseReconnectSim();
        }
    }
}
