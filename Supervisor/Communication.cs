using Modbus.Device;
using System;
using System.IO.Ports;
using System.Threading;

namespace Supervisor
{
    public abstract class Communication
    {
        public enum Functions
        {
            ReadCoils = 0x01,
            ReadDiscretes = 0x02,
            ReadHoldings = 0x03,
            ReadInputs = 0x04,
            WriteCoil = 0x05,
            WriteHolding = 0x06,
            WriteMultiCoils = 0x0F,
            WriteMultiHoldings = 0x10
        }
        public static string[] Baudrates =
        {
            "1200",
            "4800",
            "9600",
            "19200",
            "38400",
            "115200"
        };
        public static string[] DataBits =
        {
            "7",
            "8"
        };
        public static string[] StopBits =
        {
            "1",
            "2"
        };
        public static string[] Parities =
        {
            "None",
            "Odd",
            "Even"
        };
        public byte SlaveAddress { get; set; }
        public abstract event EventHandler<LogArgs> Message;
        public abstract event EventHandler<bool> StatusChanged;

        public abstract bool Connect();
        public abstract void Disconnect();
    }

    public class MasterCommunication : Communication
    {
        public bool IsConnected
        {
            get { return _IsConnected; }
            set { _IsConnected = value; StatusChanged.Invoke(this, _IsConnected); }
        }
        public override event EventHandler<LogArgs> Message = null;
        public override event EventHandler<bool> StatusChanged = null;
        public event EventHandler<ModbusMessageArgs> ReadComplete = null;
        public event EventHandler<ModbusMessageArgs> WriteComplete = null;

        private SerialPort port;
        private ModbusSerialMaster master;
        private bool _IsConnected;

        public MasterCommunication(SerialPort port)
        {
            this.port = port;
            this.port.ReadTimeout = 300;
            this.port.WriteTimeout = 300;
        }

        public override bool Connect()
        {
            if (port == null) return false;

            try
            {
                port.Open();
            }
            catch (Exception e)
            {
                Message.Invoke(this, new LogArgs(e.Message, LogArgs.LogStatus.Error));
                return false;
            }

            try
            {
                master = ModbusSerialMaster.CreateRtu(port);
            }
            catch (Exception e)
            {
                Message.Invoke(this, new LogArgs(e.Message, LogArgs.LogStatus.Error));
                return false;
            }

            Message.Invoke(this, new LogArgs($"Connected on {port.PortName}", LogArgs.LogStatus.Success));
            IsConnected = true;
            return true;
        }

        public override void Disconnect()
        {
            master.Dispose();
            if (port == null) return;
            if (port.IsOpen)
                port.Close();
            port.Dispose();
            Message.Invoke(this, new LogArgs("Disconnected", LogArgs.LogStatus.Ok));
            IsConnected = false;
        }

        public bool SendCommand(Command command)
        {
            if (!IsConnected) return false;

            switch (command.Type)
            {
                case RegisterType.Coil:
                    if (command.ReadWrite == CommandDirection.Write)
                        return WriteCoils(command, false);
                    else
                        return ReadCoils(command);

                case RegisterType.DiscreteInput:
                    return ReadDiscreteInputs(command);

                case RegisterType.HoldingRegister:
                    if (command.ReadWrite == CommandDirection.Write)
                        return WriteHoldingRegisters(command, false);
                    else
                        return ReadHoldingRegisters(command);

                case RegisterType.InputRegister:
                    return ReadInputRegisters(command);

                default:
                    return false;
            }
        }

        private bool ReadCoils(Command command)
        {
            try
            {
                var data = master.ReadCoils(SlaveAddress, command.StartAddress, command.Quantity);
                ReadComplete.Invoke(this, new ModbusMessageArgs(SlaveAddress, RegisterType.Coil, command.StartAddress, data));
                return true;
            }
            catch (Exception e)
            {
                Message.Invoke(this, new LogArgs($"Read error: {e.Message}", LogArgs.LogStatus.Error));
                IsConnected = CheckConnection();
                return false;
            }
        }

        private bool ReadDiscreteInputs(Command command)
        {
            try
            {
                var data = master.ReadInputs(SlaveAddress, command.StartAddress, command.Quantity);
                ReadComplete.Invoke(this, new ModbusMessageArgs(SlaveAddress, RegisterType.DiscreteInput, command.StartAddress, data));
                return true;
            }
            catch (Exception e)
            {
                Message.Invoke(this, new LogArgs($"Read error: {e.Message}", LogArgs.LogStatus.Error));
                IsConnected = CheckConnection();
                return false;
            }
        }

        private bool ReadHoldingRegisters(Command command)
        {
            try
            {
                var data = master.ReadHoldingRegisters(SlaveAddress, command.StartAddress, command.Quantity);
                ReadComplete.Invoke(this, new ModbusMessageArgs(SlaveAddress, RegisterType.HoldingRegister, command.StartAddress, data));
                return true;
            }
            catch (Exception e)
            {
                Message.Invoke(this, new LogArgs($"Read error: {e.Message}", LogArgs.LogStatus.Error));
                IsConnected = CheckConnection();
                return false;
            }
        }

        private bool ReadInputRegisters(Command command)
        {
            try
            {
                var data = master.ReadInputRegisters(SlaveAddress, command.StartAddress, command.Quantity);
                ReadComplete.Invoke(this, new ModbusMessageArgs(SlaveAddress, RegisterType.InputRegister, command.StartAddress, data));
                return true;
            }
            catch (Exception e)
            {
                Message.Invoke(this, new LogArgs($"Read error: {e.Message}", LogArgs.LogStatus.Error));
                IsConnected = CheckConnection();
                return false;
            }
        }

        private bool WriteCoils(Command command, bool useSingleFunction)
        {
            var data = command.GetDataToBoolArray();
            try
            {
                if (command.Quantity == 1 & useSingleFunction)
                    master.WriteSingleCoil(SlaveAddress, command.StartAddress, data[0]);
                else
                    master.WriteMultipleCoils(SlaveAddress, command.StartAddress, data);
                WriteComplete.Invoke(this, new ModbusMessageArgs(SlaveAddress, RegisterType.Coil, command.StartAddress, data));
                return true;
            }
            catch (Exception e)
            {
                Message.Invoke(this, new LogArgs($"Write error: {e.Message}", LogArgs.LogStatus.Error));
                IsConnected = CheckConnection();
                return false;
            }
        }

        private bool WriteHoldingRegisters(Command command, bool useSingleFunction)
        {
            var data = command.GetDataToUshortArray();
            try
            {
                if (command.Quantity == 1 & useSingleFunction)
                    master.WriteSingleRegister(SlaveAddress, command.StartAddress, data[0]);
                else
                    master.WriteMultipleRegisters(SlaveAddress, command.StartAddress, data);
                WriteComplete.Invoke(this, new ModbusMessageArgs(SlaveAddress, RegisterType.HoldingRegister, command.StartAddress, data));
                return true;
            }
            catch (Exception e)
            {
                Message.Invoke(this, new LogArgs($"Write error: {e.Message}", LogArgs.LogStatus.Error));
                IsConnected = CheckConnection();
                return false;
            }
        }

        private bool CheckConnection()
        {
            if (!port.IsOpen) return false;
            if (master == null) return false;

            return true;
        }
    }

    public class ModbusMessageArgs : EventArgs
    {
        public byte Slave { get; set; }
        public RegisterType Type { get; set; }
        public ushort StartAddress { get; set; }
        public short[] Data { get; set; }

        public ModbusMessageArgs(byte slave, RegisterType type, ushort start, short[] data)
        {
            Slave = slave;
            Type = type;
            StartAddress = start;
            Data = data;
        }

        public ModbusMessageArgs(byte slave, RegisterType type, ushort start, ushort[] data)
        {
            Slave = slave;
            Type = type;
            StartAddress = start;
            Data = UshortToShort(data);
        }

        public ModbusMessageArgs(byte slave, RegisterType type, ushort start, bool[] data)
        {
            Slave = slave;
            Type = type;
            StartAddress = start;
            Data = BoolToShort(data);
        }

        private short[] BoolToShort(bool[] data)
        {
            short[] result = new short[data.Length];
            for (int i = 0; i < data.Length; i++)
                result[i] = (short)(data[i] ? 1 : 0);
            return result;
        }

        private short[] UshortToShort(ushort[] data)
        {
            short[] result = new short[data.Length];
            for (int i = 0; i < data.Length; i++)
                result[i] = (short)data[i];
            return result;
        }
    }
}
