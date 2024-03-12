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

            communication = new MasterCommunication(port);
            communication.SlaveAddress = byte.Parse(SlaveText.Text);
            communication.Message += Communication_Message;
            communication.StatusChanged += Communication_StatusChanged;
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

        private void BuildUI()
        {
            ControlsStack.Content = machine;
        }

        private void CreateMachine(string path)
        {
            machine = MachineSerializer.Deserialize(path);
            if (machine == null) return;
            BuildUI();
            Created = true;
            Disconnected = true;
        }

        private void Run()
        {
            core = new MasterCore((MasterCommunication)communication, machine);
            ((MasterCore)core).OperationFailed += Core_OperationFailed;
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
            TemplateWindow tw = new TemplateWindow();
            if(tw.ShowDialog() == true)
            {
                try
                {
                    File.Copy(tw.SelectedTemplate, tw.NewFile, true);
                }
                catch(Exception exc)
                {
                    Log($"New device creation failed: {exc.Message}", LogArgs.LogStatus.Error);
                    return;
                }
                CreateMachine(tw.NewFile);
            }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "XML file | *.xml";
            ofd.DefaultExt = ".xml";
            if (ofd.ShowDialog() == true)
                CreateMachine(ofd.FileName);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            BuilderWindow bw;
            if (machine == null)
                bw = new BuilderWindow();
            else
                bw = new BuilderWindow(machine.ToArchetype());

            if (bw.ShowDialog() == true)
            {
                machine = bw.Machine.ToMachine();
                BuildUI();
            }
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
