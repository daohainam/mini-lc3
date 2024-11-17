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
        public ushort PSR { get; }
        public RegisterFile Registers { get; }
        public ReadOnlyMemory<short> Memory { get; }

        public MachineState(ushort pc, ushort ir, ushort psr, RegisterFile registers, ReadOnlyMemory<short> memory)
        {
            PC = pc;
            IR = ir;
            PSR = psr;
            Registers = registers;
            Memory = memory;
        }
    }
}
