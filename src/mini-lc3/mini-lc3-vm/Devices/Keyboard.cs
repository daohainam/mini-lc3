using mini_lc3_vm.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Devices
{
    public class Keyboard : IMappedMemory, IAttachable
    {
        public const ushort KBSR_ADDRESS = 0xFE00;
        public const ushort KBDR_ADDRESS = 0xFE02;

        public ushort KBDR { get; set; }
        public ushort KBSR { get; set; }

        private CancellationTokenSource cancellationTokenSource { get; }
        private CancellationToken cancellationToken { get; set; }

        public Keyboard()
        {
            KBDR = 0;
            KBSR = 0;

            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
        }

        public void OnReadSignal(ushort address, out short value)
        {
            if (address == KBSR_ADDRESS)
            {
                value = (short)KBSR;
            }
            else if (address == KBDR_ADDRESS)
            {
                value = (short)KBDR;
                KBSR = 0; // clear the ready bit [15]
            }
            else
            {
                value = 0;
            }
        }

        public void OnWriteSignal(ushort address, short value)
        {
            if (address == KBSR_ADDRESS)
            {
                KBSR = (ushort)value;
            }
            else if (address == KBDR_ADDRESS)
            {
                KBDR = (ushort)value;
            }
        }

        public void Attach(ILC3Machine machine)
        {
            cancellationToken = cancellationTokenSource.Token;

            Task readTask = Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    KBDR = (ushort)key.KeyChar;
                    KBSR = 0x8000; // set the ready bit [15]
                }
            }, cancellationToken);

            machine.MemoryControlUnit.Map(MemoryRange.FromStartAndEnd(Keyboard.KBSR_ADDRESS, Keyboard.KBDR_ADDRESS), new Keyboard());

            IsAttached = true;
        }

        public void Detach()
        {
            cancellationTokenSource.Cancel();
            IsAttached = false;
        }
        public bool IsAttached { get; set; } = false;
    }
}
