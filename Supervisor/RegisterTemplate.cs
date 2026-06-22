using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Supervisor
{
    [Serializable]
    public class RegisterTemplate : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string Description
        {
            get { return _Description; }
            set { _Description = value; OnPropertyChanged(); }
        }
        public string UM
        {
            get { return _UM; }
            set { _UM = value; OnPropertyChanged(); }
        }
        public List<string> States
        {
            get { return _States; }
            set { _States = value; OnPropertyChanged(); }
        }
        public bool ReadWrite
        {
            get { return _ReadWrite; }
            set { _ReadWrite = value; OnPropertyChanged(); }
        }
        public int Address
        {
            get { return _Address; }
            set { _Address = value; OnPropertyChanged(); }
        }
        public RegisterType Type
        {
            get { return _Type; }
            set { _Type = value; OnPropertyChanged(); }
        }
        public double Gain
        {
            get { return _Gain; }
            set { _Gain = value; OnPropertyChanged(); }
        }
        public short Value { get; set; }
        public double GainedValue { get; set; }

        private string _Description;
        private string _UM;
        private List<string> _States;
        private bool _ReadWrite;
        private int _Address;
        private RegisterType _Type;
        private double _Gain;

        public RegisterTemplate()
        {
            States = new List<string>();
            Gain = 1.0;
        }
        public RegisterTemplate Clone(int addressOffset)
        {
            RegisterTemplate result = new RegisterTemplate();

            result.Description = Description;
            result.UM = UM;
            result.States.AddRange(States);
            result.ReadWrite = ReadWrite;
            result.Address = Address + addressOffset;
            result.Type = Type;
            result.Gain = Gain;

            return result;
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
