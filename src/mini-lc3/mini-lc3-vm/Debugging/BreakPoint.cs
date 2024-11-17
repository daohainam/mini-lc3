using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Debugging
{
    public class BreakPoint
    {
        public ushort Address { get; set; }
        public bool Enabled { get; set; }
        public Func<MachineState, bool> Condition { get; set; }

        public BreakPoint(ushort address)
        {
            Address = address;
            Enabled = true;
            Condition = AnyState;
        }

        public BreakPoint(ushort address, Func<MachineState, bool> condition)
        {
            Address = address;
            Enabled = true;
            Condition = condition;
        }

        public static bool AnyState(MachineState state)
        {
            return true;
        }
    }
}
