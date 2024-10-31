using mini_lc3_vm.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_tests.Devices
{
    internal class MemoryConsoleDevice : IKeyboardDevice, IMonitorDevice
    {
        private Queue<byte> Input { get; } = new();
        private Queue<byte> Output { get; } = new();

        public byte Read()
        {
            if (Input.Count == 0)
            {
                throw new InvalidOperationException("No input available");
            }
            
            return Input.Dequeue();
        }

        public void Write(byte c)
        {
            Output.Enqueue(c);
        }
    }
}
