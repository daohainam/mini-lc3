using mini_lc3_vm.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Devices
{
    public class LC3Keyboard : IMappedMemory, IAttachable, IDisposable
    {
        private Guid id = Guid.NewGuid();

        public const ushort KBSR_ADDRESS = 0xFE00;
        public const ushort KBDR_ADDRESS = 0xFE02;
        private readonly IKeyboardDevice keyBoard;

        public ushort KBDR { get; set; }
        public ushort KBSR { get; set; }

        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;
        private Thread? pollThread;
        private object lockObject = new object();

        public LC3Keyboard(IKeyboardDevice keyboardDevice)
        {
            this.keyBoard = keyboardDevice;

            KBDR = 0;
            KBSR = 0;

            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
        }
        public LC3Keyboard(): this(ConsoleDevice.Instance)
        {
        }

        public void OnReadSignal(ushort address, out short value)
        {
            if (address == KBSR_ADDRESS)
            {
                lock (lockObject)
                {
                    value = (short)KBSR;
                }
            }
            else if (address == KBDR_ADDRESS)
            {
                lock (lockObject)
                {
                    value = (short)KBDR;
                    KBSR = 0; // clear the ready bit [15]
                }
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

        private void pollKeyboard()
        {
            Thread.Sleep(100);
            while (!cancellationToken.IsCancellationRequested)
            {
                if (KBSR == 0x8000) // if the ready bit is set, wait for it to be cleared
                {
                    Thread.Sleep(5);
                    continue;
                }

                var c = keyBoard.Read();
                lock (lockObject)
                {
                    KBDR = c;
                    KBSR = 0x8000; // set the ready bit [15]
                }
            }
        }

        public void Attach(ILC3Machine machine)
        {
            cancellationToken = cancellationTokenSource.Token;
            
            pollThread = new Thread(pollKeyboard);
            pollThread.Start();

            machine.MemoryControlUnit.Map(MemoryRange.FromStartAndEnd(KBSR_ADDRESS, KBDR_ADDRESS), this);

            IsAttached = true;
        }

        public void Detach()
        {
            cancellationTokenSource.Cancel();
            IsAttached = false;
        }

        public void Dispose()
        {
            if (IsAttached)
            {
                Detach();
            }
        }

        public bool IsAttached { get; set; } = false;

        
    }
}
