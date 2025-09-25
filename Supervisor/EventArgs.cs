using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supervisor
{
    public class ModbusResultArgs : EventArgs
    {
        public byte Slave { get; set; }
        public RegisterType Type { get; set; }
        public ushort StartAddress { get; set; }
        public short[] Data { get; set; }

        public ModbusResultArgs(byte slave, RegisterType type, ushort start, short[] data)
        {
            Slave = slave;
            Type = type;
            StartAddress = start;
            Data = data;
        }

        public ModbusResultArgs(byte slave, RegisterType type, ushort start, ushort[] data)
        {
            Slave = slave;
            Type = type;
            StartAddress = start;
            Data = UshortToShort(data);
        }

        public ModbusResultArgs(byte slave, RegisterType type, ushort start, bool[] data)
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

    public class LogArgs : EventArgs
    {
        public string Message { get; set; }
        public LogStatus Status { get; set; }
        public LogArgs(string message, LogStatus status)
        {
            Message = message;
            Status = status;
        }
    }
}
