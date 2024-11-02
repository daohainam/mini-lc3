using mini_lc3_vm.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Devices
{
    public class LC3Keyboard : IMappedMemory, IAttachable
    {
        private Guid id = Guid.NewGuid();

        public const ushort KBSR_ADDRESS = 0xFE00;
        public const ushort KBDR_ADDRESS = 0xFE02;
        private readonly IKeyboardDevice keyBoard;

        public LC3Keyboard(IKeyboardDevice keyboardDevice)
        {
            this.keyBoard = keyboardDevice;
        }
        public LC3Keyboard(): this(ConsoleDevice.Instance)
        {
        }

        public void OnReadSignal(ushort address, out short value)
        {
            if (address == KBSR_ADDRESS)
            {
                value = (short)(keyBoard.Ready ? 0x8000 : 0x0000);
            }
            else if (address == KBDR_ADDRESS)
            {
                value = keyBoard.Read();
            }
            else
            {
                value = 0;
            }
        }

        public void OnWriteSignal(ushort address, short value)
        {
        }

        public void Attach(ILC3Machine machine)
        {
            machine.MemoryControlUnit.Map(MemoryRange.FromStartAndEnd(KBSR_ADDRESS, KBDR_ADDRESS), this);

            IsAttached = true;
        }

        public void Detach()
        {
            IsAttached = false;
        }

        public bool IsAttached { get; set; } = false;        
    }
}
