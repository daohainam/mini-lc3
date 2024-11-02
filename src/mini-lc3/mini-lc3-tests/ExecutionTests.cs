using FluentAssertions;
using mini_lc3_vm;
using mini_lc3_vm.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_tests
{
    public class ExecutionTests
    {
        private LC3Machine _machine; 

        public ExecutionTests()
        {
            _machine = new LC3Machine();
            _machine.CPU.Boot();
        }

        [Fact]
        public void Xor_R1_R2_Test()
        {
            var instructions = new ushort[] {
                0x927F,
                0x5642,
                0x927F,
                0x94BF,
                0x5842,
                0x94BF,
                0x96FF,
                0x993F,
                0x56C4,
                0x96FF,
                0xF025
            };

            _machine.Memory.LoadInstructions(instructions);
            _machine.Memory.LoadInstructions([0x1F1F], 0x25); // HALT (TRAP x25)
            _machine.CPU.FetchAndExecuteUntilHalt();

            _machine.CPU.ALU.RegisterFile.R3.Should().Be(0x0000);
            _machine.CPU.ALU.RegisterFile.R4.Should().Be(-0x0001);
            _machine.CPU.ControlUnit.PC.Should().Be(0x1F1F);
            _machine.CPU.ControlUnit.N.Should().BeFalse();
            _machine.CPU.ControlUnit.Z.Should().BeTrue();
            _machine.CPU.ControlUnit.P.Should().BeFalse();
        }

[Fact]
        public void Reverse_String_Test()
        {
            var instructions = new ushort[] {
                0xE012,0x123F,0x6641,0x0402,0x1261,0x0FFC,0x943F,0x1481,0x14A0,0x0808,0x6600,0x6840,0x7800,
                0x7640,0x1021,0x127F,0x14BE,0x0FF6,0xF025,
                0x0041,0x0042,0x0043,0x0044,0x0045,0x0046,0x0047,0x0048,0x0049,0x004A,0x004B,0x004C,0x004D,
                0x0000
            };

            /*
                .ORIG    x3000
                rev LEA R0,FILE      ;; R0 is beginning of string
                        ADD      R1,R0,#-1    
                LOOP1 LDR R3,R1,#1     ;; Note -- LDR "looks" at the word past R1
                        BRz      DONE1
                        ADD      R1,R1,#1
                        BR       LOOP1

                DONE1 NOT R2,R0
                        ADD      R2,R2,R1

                LOOP2 ADD R2,R2,#0
                        BRn      DONE2
                        LDR      R3,R0,#0     ;; Swap
                        LDR      R4,R1,#0
                        STR      R4,R0,#0
                        STR      R3,R1,#0
                        ADD      R0,R0,#1     ;; move pointers
                        ADD      R1,R1,#-1
                        ADD      R2,R2,#-2    ;; decrease R2 by 2
                        BR       LOOP2

                DONE2 HALT

                FILE    .STRINGZ "ABCDEFGHIJKLM"
                        .END
             */

            _machine.Memory.LoadInstructions(instructions);
            _machine.Memory.LoadInstructions([0x1F1F], 0x25); // HALT (TRAP x25)
            _machine.CPU.FetchAndExecuteUntilHalt();

            _machine.CPU.ALU.RegisterFile.R0.Should().Be(0x3019);
            _machine.CPU.ALU.RegisterFile.R1.Should().Be(0x3019);
            _machine.CPU.ALU.RegisterFile.R2.Should().Be(-0x0001);
            _machine.CPU.ALU.RegisterFile.R3.Should().Be(0x46);
            _machine.CPU.ALU.RegisterFile.R4.Should().Be(0x48);
            _machine.CPU.ControlUnit.PC.Should().Be(0x1F1F);
            _machine.CPU.ControlUnit.N.Should().BeTrue();
            _machine.CPU.ControlUnit.Z.Should().BeFalse();
            _machine.CPU.ControlUnit.P.Should().BeFalse();

            var output = "ABCDEFGHIJKLM".Reverse().ToArray();
            for (int i = 0; i < output.Length; i++)
            {
                _machine.Memory[0x3013 + i].Should().Be((short)output[i]);
            }
        }        
        
        private void LoadAndExecuteInstructions(ushort[] instructions)
        {
            _machine.Memory.LoadInstructions(instructions);
            for (int i = 0; i < instructions.Length; i++)
            {
                _machine.CPU.FetchAndExecute();
            }
        }
    }
}
