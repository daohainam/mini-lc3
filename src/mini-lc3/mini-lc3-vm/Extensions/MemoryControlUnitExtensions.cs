using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Extensions;

public static class MemoryControlUnitExtensions
{
    public static void Write(this ILC3MemoryControlUnit mcu, ushort address, short value, bool isUserMode)
    {
        mcu.MAR = address;
        mcu.MDR = value;
        mcu.WriteSignal(isUserMode);
    }

    public static short Read(this ILC3MemoryControlUnit mcu, ushort address, bool isUserMode)
    {
        mcu.MAR = address;
        mcu.ReadSignal(isUserMode);
        return mcu.MDR;
    }
}
