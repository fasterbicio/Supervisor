using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Serialization;

namespace Supervisor
{
    public static class MachineSerializer
    {
        /// <summary>
        /// Carica un oggetto del file nel formato XML specificato
        /// <\summary>
        /// <param name="ObjectToLoad">Oggetto da caricare</param>
        /// <param name="XMLFilePathName">Nome del path contenente l'oggetto.</param>
        /// <returns>L'oggetto caricato,in caso di insuccesso ritorna NULL </returns>	
        private static object Deserialize(object ObjectToLoad, string XMLFilePathName)
        {
            TextReader txrTextReader = null;
            try
            {
                Type ObjectType = ObjectToLoad.GetType();
                XmlSerializer xserDocumentSerializer = new XmlSerializer(ObjectType);
                txrTextReader = new StreamReader(XMLFilePathName, Encoding.Unicode);
                ObjectToLoad = xserDocumentSerializer.Deserialize(txrTextReader);
            }
            finally
            {
                if (txrTextReader != null)
                    txrTextReader.Close();
            }
            return ObjectToLoad;
        }

        /// <summary>
        /// Salva un oggetto con la codifica specificata
        /// </summary>
        /// <param name="ObjectToSave">Oggetto da salvare</param>
        /// <param name="XMLFilePathName">Path del file</param>
        /// <returns>TRUE se salvato con successo</returns>
        private static bool Serialize(object ObjectToSave, string XMLFilePathName)
        {
            TextWriter textWriter = null;
            bool success = false;
            try
            {
                Type ObjectType = ObjectToSave.GetType();
                //Crea l'oggetto xmlSerializer con il nome del tipo da salvare
                XmlSerializer xmlSerializer = new XmlSerializer(ObjectType);
                textWriter = new StreamWriter(XMLFilePathName, false, Encoding.Unicode);
                xmlSerializer.Serialize(textWriter, ObjectToSave);
                success = true;
            }
            finally
            {
                //Chiude il file in ogni caso
                if (textWriter != null)
                    textWriter.Close();
            }

            return success;
        }

        public static Machine Deserialize(string path)
        {
            var machineArchetype = new MachineArchetype();
            machineArchetype = (MachineArchetype)Deserialize(machineArchetype, path);
            return machineArchetype.ToMachine();
        }

        public static MachineArchetype DeserializeArchetype(string path)
        {
            var machineArchetype = new MachineArchetype();
            machineArchetype = (MachineArchetype)Deserialize(machineArchetype, path);
            return machineArchetype;
        }

        public static bool Serialize(Machine machine, string path)
        {
            var machineArchetype = machine.ToArchetype();
            return Serialize(machineArchetype, path);
        }

        public static bool SerializeArchetype(MachineArchetype machineArchetype, string path)
        {
            return Serialize(machineArchetype, path);
        }
    }

    [Serializable]
    public class MachineArchetype: INotifyPropertyChanged
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
        public List<RegisterArchetype> Registers
        {
            get { return _Registers; }
            set { _Registers = value; OnPropertyChanged(); }
        }
        public List<RegisterArchetype> Settings
        {
            get { return _Settings; }
            set { _Settings = value; OnPropertyChanged(); }
        }

        private string _Model;
        private int _Type;
        private List<RegisterArchetype> _Registers;
        private List<RegisterArchetype> _Settings;

        public MachineArchetype()
        {
            Registers = new List<RegisterArchetype>();
            Settings = new List<RegisterArchetype>();
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

    [Serializable]
    public class RegisterArchetype: INotifyPropertyChanged
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

        private string _Description;
        private string _UM;
        private List<string> _States;
        private bool _ReadWrite;
        private int _Address;
        private RegisterType _Type;
        private double _Gain;

        public RegisterArchetype()
        {
            States = new List<string>();
            Gain = 1.0;
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