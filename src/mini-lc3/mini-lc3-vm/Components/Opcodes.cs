using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Components
{
    public enum Opcodes
    {
        ADD = 0b0001,
        AND = 0b0101,
        NOT = 0b1001,
        LD = 0b0010,
        LDI = 0b1010,
        LDR = 0b0110,
        LEA = 0b1110,
        ST = 0b0011,
        STI = 0b1011,
        STR = 0b0111,
        BR = 0b0000,
        JMP = 0b1100,
        JSR = 0b0100,
        RTI = 0b1000,
        TRAP = 0b1111,
        Reserved = 0b1101
    }
}
