using mini_lc3_vm.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Devices;

public class LC3Monitor : IMappedMemory, IAttachable
{
    public const ushort DSR_ADDRESS = 0xFE04;
    public const ushort DDR_ADDRESS = 0xFE06;
    private readonly IMonitorDevice monitor;

    public LC3Monitor(IMonitorDevice monitorDevice)
    {
        this.monitor = monitorDevice;
    }
    public LC3Monitor(): this(ConsoleDevice.Instance)
    {
    }

    public void OnReadSignal(ushort address, out short value)
    {
        if (address == DSR_ADDRESS)
        {
            value = (short)(monitor.Ready ? 0x8000 : 0x0000);
        }
        else if (address == DDR_ADDRESS)
        {
            value = 0;
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
            monitor.Write((byte)value);
        }
    }

    public void Attach(ILC3Machine machine)
    {
        machine.MemoryControlUnit.Map(MemoryRange.FromStartAndEnd(DSR_ADDRESS, DDR_ADDRESS), this);

        IsAttached = true;
    }

    public void Detach()
    {
        IsAttached = false;
    }
    public bool IsAttached { get; set; } = false;
}
