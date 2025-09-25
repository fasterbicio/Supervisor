using Modbus.Device;
using System;
using System.IO.Ports;

namespace Supervisor
{
    public class Communication
    {
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
        public bool IsConnected
        {
            get { return _IsConnected; }
            set { _IsConnected = value; StatusChanged.Invoke(this, _IsConnected); }
        }
        public event EventHandler<LogArgs> Message = null;
        public event EventHandler<ModbusResultArgs> ReadComplete = null;
        public event EventHandler<ModbusResultArgs> WriteComplete = null;
        public event EventHandler<bool> StatusChanged = null;

        private SerialPort port;
        private ModbusSerialMaster master;
        private bool _IsConnected;

        public Communication(SerialPort port)
        {
            this.port = port;
            this.port.ReadTimeout = 300;
            this.port.WriteTimeout = 300;
        }

        public bool Connect()
        {
            if (port == null) return false;

            try
            {
                port.Open();
            }
            catch(Exception e)
            {
                Message.Invoke(this, new LogArgs(e.Message, LogStatus.Error));
                return false;
            }

            try
            {
                master = ModbusSerialMaster.CreateRtu(port);
            }
            catch(Exception e)
            {
                Message.Invoke(this, new LogArgs(e.Message, LogStatus.Error));
                return false;
            }

            Message.Invoke(this, new LogArgs($"Connected on {port.PortName}", LogStatus.Success));
            IsConnected = true;
            return true;
        }

        public void Disconnect()
        {
            master.Dispose();
            if (port == null) return;
            if (port.IsOpen)
                port.Close();
            port.Dispose();
            Message.Invoke(this, new LogArgs("Disconnected", LogStatus.Ok));
            IsConnected = false;
        }

        public bool SendCommand( Command command)
        {
            if (!IsConnected) return false;

            switch(command.Type)
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
                ReadComplete.Invoke(this, new ModbusResultArgs(SlaveAddress, RegisterType.Coil, command.StartAddress, data));
                return true;
            }
            catch (Exception e)
            {
                Message.Invoke(this, new LogArgs($"Read error: {e.Message}", LogStatus.Error));
                IsConnected = CheckConnection();
                return false;
            }
        }

        private bool ReadDiscreteInputs(Command command)
        {
            try
            {
                var data = master.ReadInputs(SlaveAddress, command.StartAddress, command.Quantity);
                ReadComplete.Invoke(this, new ModbusResultArgs(SlaveAddress, RegisterType.DiscreteInput, command.StartAddress, data));
                return true;
            }
            catch (Exception e)
            {
                Message.Invoke(this, new LogArgs($"Read error: {e.Message}", LogStatus.Error));
                IsConnected = CheckConnection();
                return false;
            }
        }

        private bool ReadHoldingRegisters(Command command)
        {
            try
            {
                var data = master.ReadHoldingRegisters(SlaveAddress, command.StartAddress, command.Quantity);
                ReadComplete.Invoke(this, new ModbusResultArgs(SlaveAddress, RegisterType.HoldingRegister, command.StartAddress, data));
                return true;
            }
            catch (Exception e)
            {
                Message.Invoke(this, new LogArgs($"Read error: {e.Message}", LogStatus.Error));
                IsConnected = CheckConnection();
                return false;
            }
        }

        private bool ReadInputRegisters(Command command)
        {
            try
            {
                var data = master.ReadInputRegisters(SlaveAddress, command.StartAddress, command.Quantity);
                ReadComplete.Invoke(this, new ModbusResultArgs(SlaveAddress, RegisterType.InputRegister, command.StartAddress, data));
                return true;
            }
            catch (Exception e)
            {
                Message.Invoke(this, new LogArgs($"Read error: {e.Message}", LogStatus.Error));
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
                WriteComplete.Invoke(this, new ModbusResultArgs(SlaveAddress, RegisterType.Coil, command.StartAddress, data));
                return true;
            }
            catch (Exception e)
            {
                Message.Invoke(this, new LogArgs($"Write error: {e.Message}", LogStatus.Error));
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
                WriteComplete.Invoke(this, new ModbusResultArgs(SlaveAddress, RegisterType.HoldingRegister, command.StartAddress, data));
                return true;
            }
            catch (Exception e)
            {
                Message.Invoke(this, new LogArgs($"Write error: {e.Message}", LogStatus.Error));
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
}
