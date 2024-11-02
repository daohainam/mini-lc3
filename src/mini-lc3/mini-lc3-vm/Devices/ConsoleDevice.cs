using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Devices
{
    public interface IKeyboardDevice
    {
        bool Ready { get; }

        byte Read();
    }

    public interface IMonitorDevice
    {
        bool Ready { get; }

        void Write(byte c);
    }

    public class ConsoleDevice : IKeyboardDevice, IMonitorDevice
    {
        public byte Read()
        {
            return (byte)(Console.ReadKey().KeyChar & 0xFF);
        }

        public void Write(byte c)
        {
            Console.Write((char)c);
        }

        public bool Ready => true;

        public static readonly ConsoleDevice Instance = new();
    }
}
