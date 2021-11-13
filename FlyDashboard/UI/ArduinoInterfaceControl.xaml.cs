using System.Windows.Controls;
using System.IO.Ports;
using FlyDashboard.Core;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Media;
using System.Management;
using System.Linq;

namespace FlyDashboard.UI
{
    /// <summary>
    /// Interaction logic for ArduinoInterfaceControl.xaml
    /// </summary>
    public partial class ArduinoInterfaceControl : UserControl, INotifyPropertyChanged
    {
        private ArduinoCommunicator _communicator;

        public event PropertyChangedEventHandler? PropertyChanged;

        private Brush _ledColor;
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

        public ArduinoCommunicator Communicator { get => _communicator; }

        public ArduinoInterfaceControl()
        {
            InitializeComponent();

            DataContext = this;

            FillPorts();

            LedColor = new SolidColorBrush(Colors.Red);

            GenericEvents.OnConnectDashboard += GenericEvents_OnConnectDashboard;
            GenericEvents.OnDisconnectDashboard += GenericEvents_OnDisconnectDashboard; ;

            _communicator = new ArduinoCommunicator();
        }

        private void GenericEvents_OnDisconnectDashboard(object? sender, System.EventArgs e)
        {
            LedColor = new SolidColorBrush(Colors.Red);
            connectButtonTextBlock.Text = "Connect HW";
        }

        private void GenericEvents_OnConnectDashboard(object? sender, System.EventArgs e)
        {
            LedColor = new SolidColorBrush(Colors.Green);
            connectButtonTextBlock.Text = "Disconnect HW";
        }

        private void FillPorts()
        {
            serialPortsComboBox.Items.Clear();

            using (var searcher = new ManagementObjectSearcher
                ("SELECT * FROM WIN32_SerialPort"))
            {
                string[] portnames = SerialPort.GetPortNames();
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
                var tList = (from n in portnames
                             join p in ports on n equals p["DeviceID"].ToString()
                             select n + " - " + p["Caption"]).ToList();

                foreach(var t in tList)
                {
                    serialPortsComboBox.Items.Add(t.ToString());
                }
            }

            if (serialPortsComboBox.Items.Count > 0)
            {
                serialPortsComboBox.IsEnabled = true;
                serialPortsComboBox.SelectedIndex = 0;
            }
        }

        private void SerialPortsConnect_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if(_communicator.IsConnected)
            {
                _communicator.Disconnect();
            }
            else
            {
                string selectedComboBox = (string)serialPortsComboBox.SelectedItem;
                string port = selectedComboBox.Substring(0, selectedComboBox.IndexOf("-")).Trim();

                _communicator.Connect(port);
            }
        }

        private void SerialPortsComboBox_DropDownOpened(object sender, System.EventArgs e)
        {
            FillPorts();
        }
    }
}
