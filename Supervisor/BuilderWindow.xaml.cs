using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Supervisor
{
    /// <summary>
    /// Logica di interazione per BuilderWindow.xaml
    /// </summary>
    public partial class BuilderWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public MachineTemplate Machine
        {
            get { return _Machine; }
            set { _Machine = value; OnPropertyChanged(); }
        }
        public List<RegisterTemplate> Registers
        {
            get { return _Registers; }
            set { _Registers = value; OnPropertyChanged(); }
        }
        public int SelectedRegister
        {
            get { return _SelectedRegister; }
            set { _SelectedRegister = value; OnPropertyChanged(); RefreshEditor(); }
        }

        private MachineTemplate _Machine;
        private int _SelectedRegister;
        private List<RegisterTemplate> _Registers;

        public BuilderWindow()
        {
            InitializeComponent();

            Machine = new MachineTemplate();
            Registers = new List<RegisterTemplate>();

            DataContext = this;
        }

        public BuilderWindow(MachineTemplate machine)
        {
            InitializeComponent();

            Machine = machine;
            Registers = machine.Registers.Concat(machine.Settings).ToList();
            Sort();

            DataContext = this;
        }

        private int Add()
        {
            RegisterTemplate register = new RegisterTemplate();
            Registers.Add(register);
            return Registers.Count - 1;
        }

        private int Delete(int index)
        {
            Registers.RemoveAt(index);
            return Registers.Count - 1;
        }

        private void Import(string filename)
        {
            Machine = MachineSerializer.DeserializeTemplate(filename);
            Registers = Machine.Registers.Concat(Machine.Settings).ToList();
            Sort();
        }

        private bool Export(string filename)
        {
            AssignToMachine();
            return MachineSerializer.SerializeTemplate(Machine, filename);
        }

        private void Sort()
        {
            Registers = Registers.OrderByDescending(o => o.Type).ThenBy(o => o.Address).ToList();
        }

        private void RefreshUI()
        {
            if (Registers == null) return;
            ListviewRegisters.ItemsSource = null;
            ListviewRegisters.ItemsSource = Registers;
        }

        private void RefreshEditor()
        {
            if (SelectedRegister < 0) return;
            Editor.DataContext = null;
            if (Registers.Count > 0)
                Editor.DataContext = Registers[SelectedRegister];
        }

        private void AssignToMachine()
        {
            Machine.Registers.Clear();
            Machine.Settings.Clear();

            for (int i = 0; i < Registers.Count; i++)
            {
                if (Registers[i].ReadWrite)
                    Machine.Settings.Add(Registers[i]);
                else
                    Machine.Registers.Add(Registers[i]);
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            AssignToMachine();
            DialogResult = true;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Add();
            Sort();
            RefreshUI();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Delete(SelectedRegister);
            RefreshUI();
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "XML file | *.xml";
            ofd.DefaultExt = ".xml";
            if (ofd.ShowDialog() == true)
                Import(ofd.FileName);
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "XML file | *.xml";
            sfd.DefaultExt = ".xml";
            if (sfd.ShowDialog() == true)
                Export(sfd.FileName);
        }

        private void Sort_Click(object sender, RoutedEventArgs e)
        {
            Sort();
            RefreshUI();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshUI();
            RefreshEditor();
        }
    }
}
