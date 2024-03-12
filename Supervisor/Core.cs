using Supervisor.Properties;
using System;
using System.Collections.Generic;
using System.Timers;

namespace Supervisor
{
    public abstract class Core
    {
        public abstract void Start();
        public abstract void Stop();
    }

    public class MasterCore : Core
    {
        public bool Active { get; set; }
        public event EventHandler OperationFailed = null;

        private const double mainTime = 100;
        private int retries = 0;

        private MasterCommunication communication;
        private Machine machine;
        private Timer mainTimer;
        private List<Command> stack;
        private CoreStatus status;

        private enum CoreStatus
        {
            Init,
            WriteCoils,
            ReadCoils,
            ReadDiscreteInputs,
            WriteHoldingRegisters,
            ReadHoldingRegisters,
            ReadInputRegisters
        }

        public MasterCore(MasterCommunication communication, Machine machine)
        {
            mainTimer = new Timer(mainTime);
            mainTimer.Elapsed += MainTimer_Elapsed;

            this.communication = communication;
            this.communication.ReadComplete += Communication_ReadComplete;
            this.communication.WriteComplete += Communication_WriteComplete;

            this.machine = machine;
        }

        public override void Start()
        {
            if (!communication.IsConnected) return;

            mainTimer.Stop();
            status = CoreStatus.Init;
            Active = true;
            mainTimer.Start();
        }

        public override void Stop()
        {
            mainTimer.Stop();
            Active = false;
        }



        private void CoreFsm()
        {
            if (!communication.IsConnected)
                Stop();

            if (stack != null)
            {
                if (stack.Count > 0)
                {
                    StackFsm();
                    return;
                }
            }

            switch (status)
            {
                case CoreStatus.Init:
                    stack = machine.GetCoilsWrite();
                    status = CoreStatus.WriteCoils;
                    break;

                case CoreStatus.WriteCoils:
                    stack = machine.GetCoilsRead();
                    status = CoreStatus.ReadCoils;
                    break;

                case CoreStatus.ReadCoils:
                    stack = machine.GetDiscretesRead();
                    status = CoreStatus.ReadDiscreteInputs;
                    break;

                case CoreStatus.ReadDiscreteInputs:
                    stack = machine.GetHoldingsWrite();
                    status = CoreStatus.WriteHoldingRegisters;
                    break;

                case CoreStatus.WriteHoldingRegisters:
                    stack = machine.GetHoldingsRead();
                    status = CoreStatus.ReadHoldingRegisters;
                    break;

                case CoreStatus.ReadHoldingRegisters:
                    stack = machine.GetInputsRead();
                    status = CoreStatus.ReadInputRegisters;
                    break;

                case CoreStatus.ReadInputRegisters:
                    stack = machine.GetCoilsWrite();
                    status = CoreStatus.WriteCoils;
                    break;
            }
        }

        private void StackFsm()
        {
            if (stack.Count == 0) return;
            if (!communication.IsConnected) return;

            var command = stack[0];
            bool remove = communication.SendCommand(command);

            if (!remove)
            {
                retries++;
                if (retries < Settings.Default.Retries)
                    return;
                else
                    OperationFailed.Invoke(this, null);
            }

            retries = 0;
            if (stack.Count > 0)
                stack.RemoveAt(0);
        }

        private void MainTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CoreFsm();
        }

        private void Communication_WriteComplete(object sender, ModbusMessageArgs e)
        {
            machine.ResetFlags(e);
        }

        private void Communication_ReadComplete(object sender, ModbusMessageArgs e)
        {
            machine.StoreData(e);
        }
    }
}
