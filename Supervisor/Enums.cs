using System;

namespace Supervisor
{
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

    public enum LogStatus
    {
        Ok,
        Error,
        Success
    }
}
