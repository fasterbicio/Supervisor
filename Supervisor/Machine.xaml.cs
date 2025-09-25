using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace Supervisor
{
    /// <summary>
    /// Logica di interazione per Machine.xaml
    /// </summary>
    public partial class Machine : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string Model { get; set; }
        public int Type { get; set; }
        public List<Register> Registers { get; set; }
        public List<Register> Settings { get; set; }
        public bool IsModified { get; set; }

        public Machine()
        {
            InitializeComponent();
            Registers = new List<Register>();
            Settings = new List<Register>();
            DataContext = this;
            LinkEvents();
        }
        public List<Command> GetCoilsWrite()
        {
            List<Command> coilsWrite = new List<Command>();

            for (int i = 0; i < Settings.Count; i++)
            {
                if (Settings[i].Type == RegisterType.Coil)
                {
                    if (Settings[i].Modified)
                    {
                        Settings[i].Keep = false;

                        var command = new Command();
                        command.Type = RegisterType.Coil;
                        command.ReadWrite = CommandDirection.Write;
                        command.StartAddress = (ushort)Settings[i].Address;
                        command.Quantity = 1;
                        command.Data.Add(Settings[i].Value);
                        coilsWrite.Add(command);
                    }
                }
            }

            return Command.MergeCommands(Command.SortCommands(coilsWrite));
        }
        public List<Command> GetCoilsRead()
        {
            List<Command> coilsRead = new List<Command>();

            for (int i = 0; i < Registers.Count; i++)
            {
                if (Registers[i].Type == RegisterType.Coil)
                {
                    if (!Registers[i].Modified)
                    {
                        var command = new Command();
                        command.Type = RegisterType.Coil;
                        command.ReadWrite = CommandDirection.Read;
                        command.StartAddress = (ushort)Registers[i].Address;
                        command.Quantity = 1;
                        coilsRead.Add(command);
                    }
                }
            }
            for (int i = 0; i < Settings.Count; i++)
            {
                if (Settings[i].Type == RegisterType.Coil)
                {
                    if (!Settings[i].Modified)
                    {
                        var command = new Command();
                        command.Type = RegisterType.Coil;
                        command.ReadWrite = CommandDirection.Read;
                        command.StartAddress = (ushort)Settings[i].Address;
                        command.Quantity = 1;
                        coilsRead.Add(command);
                    }
                }
            }
            return Command.MergeCommands(Command.SortCommands(coilsRead));
        }
        public List<Command> GetDiscretesRead()
        {
            List<Command> discretesRead = new List<Command>();

            for (int i = 0; i < Registers.Count; i++)
            {
                if (Registers[i].Type == RegisterType.DiscreteInput)
                {
                    var command = new Command();
                    command.Type = RegisterType.DiscreteInput;
                    command.ReadWrite = CommandDirection.Read;
                    command.StartAddress = (ushort)Registers[i].Address;
                    command.Quantity = 1;
                    discretesRead.Add(command);
                }
            }
            return Command.MergeCommands(Command.SortCommands(discretesRead));
        }
        public List<Command> GetHoldingsWrite()
        {
            List<Command> holdingsWrite = new List<Command>();

            for (int i = 0; i < Settings.Count; i++)
            {
                if (Settings[i].Type == RegisterType.HoldingRegister)
                {
                    if (Settings[i].Modified)
                    {
                        Settings[i].Keep = false;

                        var command = new Command();
                        command.Type = RegisterType.HoldingRegister;
                        command.ReadWrite = CommandDirection.Write;
                        command.StartAddress = (ushort)Settings[i].Address;
                        command.Quantity = 1;
                        command.Data.Add(Settings[i].Value);
                        holdingsWrite.Add(command);
                    }
                }
            }
            return Command.MergeCommands(Command.SortCommands(holdingsWrite));
        }
        public List<Command> GetHoldingsRead()
        {
            List<Command> holdingsRead = new List<Command>();

            for (int i = 0; i < Registers.Count; i++)
            {
                if (Registers[i].Type == RegisterType.HoldingRegister)
                {
                    if (!Registers[i].Keep)
                    {
                        if (!Registers[i].Modified)
                        {
                            var command = new Command();
                            command.Type = RegisterType.HoldingRegister;
                            command.ReadWrite = CommandDirection.Read;
                            command.StartAddress = (ushort)Registers[i].Address;
                            command.Quantity = 1;
                            holdingsRead.Add(command);
                        }
                    }
                }
            }
            for (int i = 0; i < Settings.Count; i++)
            {
                if (Settings[i].Type == RegisterType.HoldingRegister)
                {
                    if (!Settings[i].Keep)
                    {
                        if (!Settings[i].Modified)
                        {
                            var command = new Command();
                            command.Type = RegisterType.HoldingRegister;
                            command.ReadWrite = CommandDirection.Read;
                            command.StartAddress = (ushort)Settings[i].Address;
                            command.Quantity = 1;
                            holdingsRead.Add(command);
                        }
                    }
                }
            }
            return Command.MergeCommands(Command.SortCommands(holdingsRead));
        }
        public List<Command> GetInputsRead()
        {
            List<Command> inputsRead = new List<Command>();

            for (int i = 0; i < Registers.Count; i++)
            {
                if (Registers[i].Type == RegisterType.InputRegister)
                {
                    var command = new Command();
                    command.Type = RegisterType.HoldingRegister;
                    command.ReadWrite = CommandDirection.Read;
                    command.StartAddress = (ushort)Registers[i].Address;
                    command.Quantity = 1;
                    inputsRead.Add(command);
                }
            }
            return Command.MergeCommands(Command.SortCommands(inputsRead));
        }
        public void StoreData(ModbusResultArgs result)
        {
            for (int i = 0; i < result.Data.Length; i++)
                StoreValue(result.Type, result.StartAddress + i, result.Data[i]);
        }
        public void ResetFlags(ModbusResultArgs result)
        {
            for (int i = 0; i < result.Data.Length; i++)
                ResetFlag(result.Type, result.StartAddress + i);
        }
        private void StoreValue(RegisterType type, int address, short value)
        {
            for (int i = 0; i < Registers.Count; i++)
            {
                if (Registers[i].Address == address)
                {
                    if (Registers[i].Type == type)
                    {
                        if (!Registers[i].Keep)
                            Registers[i].SetValue(value);
                    }
                }
            }
            for (int i = 0; i < Settings.Count; i++)
            {
                if (Settings[i].Address == address)
                {
                    if (Settings[i].Type == type)
                    {
                        if (!Settings[i].Keep)
                            Settings[i].SetValue(value);
                    }
                }
            }
        }
        private void ResetFlag(RegisterType type, int address)
        {
            for (int i = 0; i < Settings.Count; i++)
            {
                if (Settings[i].Address == address)
                {
                    if (Settings[i].Type == type)
                        Settings[i].Modified = false;
                }
            }
        }
        private void LinkEvents()
        {
            foreach(Register reg in Registers)
            {
                reg.IsModified += Register_IsModified;
            }
            foreach(Register reg in Settings)
            {
                reg.IsModified += Register_IsModified;
            }
        }
        private void Register_IsModified(object sender, EventArgs e)
        {
            IsModified = true;
        }
        public MachineTemplate ToTemplate()
        {
            MachineTemplate machine = new MachineTemplate();

            machine.Model = Model;
            machine.Type = Type;

            for (int i = 0; i < Registers.Count; i++)
            {
                RegisterTemplate register = new RegisterTemplate();

                register.Description = Registers[i].Description;
                register.UM = Registers[i].UM;
                register.ReadWrite = Registers[i].ReadWrite;
                register.Gain = Registers[i].Gain;
                register.Address = Registers[i].Address;
                register.Type = Registers[i].Type;
                register.States.AddRange(Registers[i].States);
                register.Value = Registers[i].Value;
                register.GainedValue = Registers[i].GainedValue;

                machine.Registers.Add(register);
            }
            for (int i = 0; i < Settings.Count; i++)
            {
                RegisterTemplate register = new RegisterTemplate();

                register.Description = Settings[i].Description;
                register.UM = Settings[i].UM;
                register.ReadWrite = Settings[i].ReadWrite;
                register.Gain = Settings[i].Gain;
                register.Address = Settings[i].Address;
                register.Type = Settings[i].Type;
                register.States.AddRange(Settings[i].States);
                register.Value = Settings[i].Value;
                register.GainedValue = Settings[i].GainedValue;

                machine.Settings.Add(register);
            }

            return machine;
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
}
