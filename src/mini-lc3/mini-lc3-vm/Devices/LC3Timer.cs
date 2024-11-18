using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Devices;
public class LC3Timer : IMappedMemory, IAttachable
{
    public const ushort TMR_ADDRESS = 0xFE08;
    public const ushort TMI_ADDRESS = 0xFE0A;

    private ushort TMR = 0;
    private ushort TMI = 0;
    private DateTime lastTime = DateTime.Now;

    public void OnReadSignal(ushort address, out short value)
    {
        if (address == TMR_ADDRESS)
        {
            if (TMI == 0) {
                value = 0;
            }
            else
            {
                if ((TMR & 0x8000) == 0x8000)
                {
                    value = (short)TMR;
                }
                else
                {
                    var currentTime = DateTime.Now;
                    var elapsed = currentTime - lastTime;
                    var elapsedMilliseconds = (ushort)elapsed.TotalMilliseconds;
                    TMR = (ushort)((elapsedMilliseconds > TMI ? 0x8000 : 0x0000) | TMR);
                    lastTime = currentTime;

                    value = (short)TMR;
                }
            }
        }
        else if (address == TMI_ADDRESS)
        {
            value = (short)TMI;
        }
        else
        {
            value = 0;
        }
    }

    public void OnWriteSignal(ushort address, short value)
    {
        if (address == TMR_ADDRESS)
        {
            TMR = (ushort)value;
        }
        else if (address == TMI_ADDRESS)
        {
            TMI = (ushort)value;
        }
    }

    public void Attach(ILC3Machine machine)
    {
        machine.MemoryControlUnit.Map(MemoryRange.FromStartAndEnd(TMR_ADDRESS, TMI_ADDRESS), this);

        lastTime = DateTime.Now;
        IsAttached = true;
    }

    public void Detach()
    {
        IsAttached = false;
    }
    public bool IsAttached { get; set; } = false;
}
