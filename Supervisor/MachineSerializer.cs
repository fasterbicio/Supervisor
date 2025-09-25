using System;
using System.IO;
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
            var machineTemplate = new MachineTemplate();
            machineTemplate = (MachineTemplate)Deserialize(machineTemplate, path);
            return machineTemplate.ToMachine();
        }

        public static MachineTemplate DeserializeTemplate(string path)
        {
            var machineTemplate = new MachineTemplate();
            machineTemplate = (MachineTemplate)Deserialize(machineTemplate, path);
            return machineTemplate;
        }

        public static bool Serialize(Machine machine, string path)
        {
            var machineTemplate = machine.ToTemplate();
            return Serialize(machineTemplate, path);
        }

        public static bool SerializeTemplate(MachineTemplate machineTemplate, string path)
        {
            return Serialize(machineTemplate, path);
        }
    }
}