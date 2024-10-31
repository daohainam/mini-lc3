using mini_lc3_vm.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Devices
{
    public class Monitor : IMappedMemory, IAttachable
    {
        public const ushort DSR_ADDRESS = 0xFE04;
        public const ushort DDR_ADDRESS = 0xFE06;
        private readonly IMonitorDevice monitor;

        public ushort DDR { get; set; }
        public ushort DSR { get; set; }

        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;
        private Task task = Task.CompletedTask;
        private AutoResetEvent autoResetEvent = new(false);

        public Monitor(IMonitorDevice monitorDevice)
        {
            this.monitor = monitorDevice;

            DDR = 0;
            DSR = 0;

            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
        }
        public Monitor(): this(ConsoleDevice.Instance)
        {
        }

        public void OnReadSignal(ushort address, out short value)
        {
            if (address == DSR_ADDRESS)
            {
                value = (short)DSR;
            }
            else if (address == DDR_ADDRESS)
            {
                value = (short)DDR;
            }
            else
            {
                value = 0;
            }
        }

        public void OnWriteSignal(ushort address, short value)
        {
            if (address == DDR_ADDRESS)
            {
                DDR = (ushort)value;
                DSR = 0; // clear the ready bit [15]

                autoResetEvent.Set();
            }
        }

        public void Attach(ILC3Machine machine)
        {
            cancellationToken = cancellationTokenSource.Token;

            task = Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (autoResetEvent.WaitOne(100))
                    {
                        monitor.Write((byte)DDR);
                        DSR = 0x8000; // set the ready bit [15]
                    }
                }
            }, cancellationToken);

            machine.MemoryControlUnit.Map(MemoryRange.FromStartAndEnd(DSR_ADDRESS, DDR_ADDRESS), new Keyboard());

            IsAttached = true;
        }

        public void Detach()
        {
            cancellationTokenSource.Cancel();
            task.Wait();
            IsAttached = false;
        }
        public bool IsAttached { get; set; } = false;
    }
}
