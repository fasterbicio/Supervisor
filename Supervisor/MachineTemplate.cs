using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Supervisor
{
    [Serializable]
    public class MachineTemplate : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string Model
        {
            get { return _Model; }
            set { _Model = value; OnPropertyChanged(); }
        }
        public int Type
        {
            get { return _Type; }
            set { _Type = value; OnPropertyChanged(); }
        }
        public List<RegisterTemplate> Registers
        {
            get { return _Registers; }
            set { _Registers = value; OnPropertyChanged(); }
        }
        public List<RegisterTemplate> Settings
        {
            get { return _Settings; }
            set { _Settings = value; OnPropertyChanged(); }
        }

        private string _Model;
        private int _Type;
        private List<RegisterTemplate> _Registers;
        private List<RegisterTemplate> _Settings;

        public MachineTemplate()
        {
            Registers = new List<RegisterTemplate>();
            Settings = new List<RegisterTemplate>();
        }
        public Machine ToMachine()
        {
            Machine machine = new Machine();

            machine.Model = Model;
            machine.Type = Type;

            for (int i = 0; i < Registers.Count; i++)
            {
                Register register = new Register();

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
                Register register = new Register();

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
        public void Sort()
        {
            Registers = Registers.OrderByDescending(o => o.Type).ThenBy(o => o.Address).ToList();
            Settings = Settings.OrderByDescending(o => o.Type).ThenBy(o => o.Address).ToList();
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
