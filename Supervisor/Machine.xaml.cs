using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        public Machine()
        {
            InitializeComponent();
            Registers = new List<Register>();
            Settings = new List<Register>();
            DataContext = this;
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
                        Settings[i].Freeze = false;

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
                        Settings[i].Freeze = false;

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
            for (int i = 0; i < Settings.Count; i++)
            {
                if (Settings[i].Type == RegisterType.HoldingRegister)
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
                        if (!Registers[i].Freeze)
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
                        if (!Settings[i].Freeze)
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
        public MachineArchetype ToArchetype()
        {
            MachineArchetype machine = new MachineArchetype();

            machine.Model = Model;
            machine.Type = Type;

            for (int i = 0; i < Registers.Count; i++)
            {
                RegisterArchetype register = new RegisterArchetype();

                register.Description = Registers[i].Description;
                register.UM = Registers[i].UM;
                register.ReadWrite = Registers[i].ReadWrite;
                register.Gain = Registers[i].Gain;
                register.Address = Registers[i].Address;
                register.Type = Registers[i].Type;
                register.States.AddRange(Registers[i].States);

                machine.Registers.Add(register);
            }
            for (int i = 0; i < Settings.Count; i++)
            {
                RegisterArchetype register = new RegisterArchetype();

                register.Description = Settings[i].Description;
                register.UM = Settings[i].UM;
                register.ReadWrite = Settings[i].ReadWrite;
                register.Gain = Settings[i].Gain;
                register.Address = Settings[i].Address;
                register.Type = Settings[i].Type;
                register.States.AddRange(Settings[i].States);

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

    [Serializable]
    public enum RegisterType
    {
        Coil,
        DiscreteInput,
        HoldingRegister,
        InputRegister
    }

    public enum CommandDirection
    {
        Read,
        Write
    }

    public class Command
    {
        public RegisterType Type { get; set; }
        public CommandDirection ReadWrite { get; set; }
        public ushort StartAddress { get; set; }
        public ushort Quantity { get; set; }
        public List<short> Data { get; set; }
        public Command()
        {
            Data = new List<short>();
        }
        public bool[] GetDataToBoolArray()
        {
            bool[] data = new bool[Data.Count];
            for (int i = 0; i < Data.Count; i++)
                data[i] = Data[i] != 0;
            return data;
        }
        public ushort[] GetDataToUshortArray()
        {
            ushort[] data = new ushort[Data.Count];
            for (int i = 0; i < Data.Count; i++)
                data[i] = (ushort)Data[i];
            return data;
        }
        public static List<Command> SortCommands(List<Command> commands)
        {
            return commands.OrderBy(o => o.Type).ThenBy(o => o.StartAddress).ToList();
        }
        public static List<Command> MergeCommands(List<Command> commands)
        {
            if (commands.Count <= 1) return commands;

            var result = commands;
            for (int i = 0, j = 1; j < result.Count; i++, j++)
            {
                while (j < result.Count && result[i].StartAddress + result[i].Quantity == result[j].StartAddress & result[i].ReadWrite == result[j].ReadWrite)
                {
                    result[i].Quantity++;
                    if (result[i].ReadWrite == CommandDirection.Write)
                        result[i].Data.Add(result[j].Data[0]);
                    result.RemoveAt(j);
                    if (result.Count <= 1) break;
                }
                j = i + 1;
            }

            return result;
        }
    }
}
