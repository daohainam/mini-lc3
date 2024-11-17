using mini_lc3_vm.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Debugging
{
    public class MachineState
    {
        public ushort PC { get; }
        public ushort IR { get; }
        public ushort MAR { get; }
        public ushort MDR { get; }
        public RegisterFile Registers { get; }
        public ReadOnlyMemory<short> Memory { get; }

        public MachineState(ushort pc, ushort ir, ushort mar, ushort mdr, RegisterFile registers, ReadOnlyMemory<short> memory)
        {
            PC = pc;
            IR = ir;
            MAR = mar;
            MDR = mdr;
            Registers = registers;
            Memory = memory;
        }
    }
}
