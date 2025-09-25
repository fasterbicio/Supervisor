using System.Collections.Generic;
using System.Linq;

namespace Supervisor
{
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
