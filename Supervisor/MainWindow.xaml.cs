using Microsoft.Win32;
using Supervisor.Properties;
using System;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Media;

namespace Supervisor
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public bool Created
        {
            get { return _Created; }
            set { _Created = value; OnPropertyChanged(); }
        }
        public bool Connected
        {
            get { return _Connected; }
            set { _Connected = value; OnPropertyChanged(); }
        }
        public bool Disconnected
        {
            get { return _Created & _Disconnected; }
            set { _Disconnected = value; OnPropertyChanged(); }
        }

        private bool _Created;
        private bool _Connected;
        private bool _Disconnected;
        private Timer logTimer;
        private Communication communication;
        private Core core;
        private Machine machine;
        private string[] ports;

        public MainWindow()
        {
            InitializeComponent();

            logTimer = new Timer(5000);
            logTimer.Elapsed += LogTimer_Elapsed;

            RefreshInfo();

            DataContext = this;
        }

        private bool Connect()
        {
            if (ports.Length == 0) return false;

            Settings.Default.Port = ports[PortsCombo.SelectedIndex];
            Settings.Default.Baudrate = BaudCombo.SelectedIndex;
            Settings.Default.DataBits = DataCombo.SelectedIndex;
            Settings.Default.StopBits = StopCombo.SelectedIndex;
            Settings.Default.Parity = ParityCombo.SelectedIndex;
            Settings.Default.SlaveAddress = int.Parse(SlaveText.Text);
            Settings.Default.Timeout = int.Parse(TimeoutText.Text);
            Settings.Default.Retries = int.Parse(RetriesText.Text);
            Settings.Default.Save();

            SerialPort port = new SerialPort()
            {
                PortName = ports[PortsCombo.SelectedIndex],
                BaudRate = int.Parse(BaudCombo.SelectedItem as string),
                DataBits = int.Parse(DataCombo.SelectedItem as string),
                StopBits = (StopBits)(StopCombo.SelectedIndex + 1),
                Parity = (Parity)ParityCombo.SelectedIndex,
                ReadTimeout = Settings.Default.Timeout,
                WriteTimeout = Settings.Default.Timeout
            };

            communication = new Communication(port);
            communication.Message += Communication_Message;
            communication.StatusChanged += Communication_StatusChanged;
            communication.SlaveAddress = byte.Parse(SlaveText.Text);
            return communication.Connect();
        }

        private void Disconnect()
        {
            communication.Disconnect();
        }

        private void RefreshInfo()
        {
            ports = SerialPort.GetPortNames();
            PortsCombo.ItemsSource = ports;

            for (int i = 0; i < ports.Length; i++)
            {
                if (Settings.Default.Port.Equals(ports[i]))
                    PortsCombo.SelectedIndex = i;
            }

            BaudCombo.ItemsSource = Communication.Baudrates;
            BaudCombo.SelectedIndex = Settings.Default.Baudrate;

            DataCombo.ItemsSource = Communication.DataBits;
            DataCombo.SelectedIndex = Settings.Default.DataBits;

            StopCombo.ItemsSource = Communication.StopBits;
            StopCombo.SelectedIndex = Settings.Default.StopBits;

            ParityCombo.ItemsSource = Communication.Parities;
            ParityCombo.SelectedIndex = Settings.Default.Parity;

            SlaveText.Text = Settings.Default.SlaveAddress.ToString();

            TimeoutText.Text = Settings.Default.Timeout.ToString();

            RetriesText.Text = Settings.Default.Retries.ToString();
        }

        private void Log(string message, LogArgs.LogStatus color)
        {
            Dispatcher.Invoke(() =>
            {
                LogLabel.Content = $"{DateTime.Now:HH:mm:ss} - {message}";
                switch (color)
                {
                    case LogArgs.LogStatus.Ok:
                        LogBar.Background = Brushes.White;
                        LogLabel.Foreground = Brushes.Black;
                        break;
                    case LogArgs.LogStatus.Error:
                        LogBar.Background = Brushes.OrangeRed;
                        LogLabel.Foreground = Brushes.White;
                        break;
                    case LogArgs.LogStatus.Success:
                        LogBar.Background = Brushes.LightGreen;
                        LogLabel.Foreground = Brushes.Black;
                        break;
                }
            });
            logTimer.Stop();
            logTimer.Start();
        }

        private void ResetLogBar()
        {
            Dispatcher.Invoke(() =>
            {
                LogBar.Background = Brushes.White;
                LogLabel.Foreground = Brushes.Black;
            });
        }

        private void OpenBuilder()
        {
            BuilderWindow bw;
            if (machine == null)
                bw = new BuilderWindow();
            else
                bw = new BuilderWindow(machine.ToTemplate());

            if (bw.ShowDialog() == true)
            {
                machine = bw.Machine.ToMachine();
                BuildUI();
            }
        }

        private void BuildUI()
        {
            ControlsStack.Content = machine;
        }

        private bool CreateMachine(string path)
        {
            machine = MachineSerializer.Deserialize(path);
            machine.Model = Path.GetFileNameWithoutExtension(path);
            if (machine == null) return false;
            BuildUI();
            Created = true;
            Disconnected = true;
            return true;
        }

        private bool SaveSettings(string path)
        {
            MachineTemplate machine = this.machine.ToTemplate();
            MachineTemplate settingsMachine = new MachineTemplate();

            settingsMachine.Model = machine.Model;
            foreach(RegisterTemplate reg in machine.Settings)
            {
                settingsMachine.Settings.Add(reg);
            }

            return MachineSerializer.SerializeTemplate(settingsMachine, path);
        }

        private void Run()
        {
            core = new Core(communication, machine);
            core.OperationFailed += Core_OperationFailed;
            core.Start();
        }

        private void Stop()
        {
            core.Stop();
        }



        private void LogTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            logTimer.Stop();
            ResetLogBar();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int v1, v2;
            v1 = Assembly.GetExecutingAssembly().GetName().Version.Major;
            v2 = Assembly.GetExecutingAssembly().GetName().Version.Minor;

            Title = $"Supervisor {v1}.{v2}";
            Log("Ready", LogArgs.LogStatus.Ok);
        }

        private void Communication_Message(object sender, LogArgs e)
        {
            Log(e.Message, e.Status);
        }

        private void Communication_StatusChanged(object sender, bool e)
        {
            Connected = e;
            Disconnected = !Connected;
        }

        private void Core_OperationFailed(object sender, EventArgs e)
        {
            Stop();
            Disconnect();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Connect()) return;
            Run();
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            Stop();
            Disconnect();
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            machine = null;
            OpenBuilder();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "XML file | *.xml";
            ofd.DefaultExt = ".xml";
            if (ofd.ShowDialog() == true)
            {
                if (!CreateMachine(ofd.FileName))
                    Log($"Device loading failed", LogArgs.LogStatus.Error);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            OpenBuilder();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "SETTINGS file | *.settings";
            sfd.DefaultExt = ".settings";
            sfd.FileName = $"{machine.Model}_{DateTime.Now:yyyyMMddHHmmss}";
            if (sfd.ShowDialog() == true)
            {
                SaveSettings(sfd.FileName);
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {

        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
    }

    public class LogArgs : EventArgs
    {
        public enum LogStatus
        {
            Ok,
            Error,
            Success
        }
        public string Message { get; set; }
        public LogStatus Status { get; set; }
        public LogArgs(string message, LogStatus status)
        {
            Message = message;
            Status = status;
        }
    }
}
