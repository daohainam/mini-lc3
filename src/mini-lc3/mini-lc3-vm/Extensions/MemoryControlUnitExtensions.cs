using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Extensions
{
    public static class MemoryControlUnitExtensions
    {
        public static void Write(this ILC3MemoryControlUnit mcu, ushort address, short value)
        {
            mcu.MAR = address;
            mcu.MDR = value;
            mcu.WriteSignal();
        }

        public static short Read(this ILC3MemoryControlUnit mcu, ushort address)
        {
            mcu.MAR = address;
            mcu.ReadSignal();
            return mcu.MDR;
        }
    }
}
