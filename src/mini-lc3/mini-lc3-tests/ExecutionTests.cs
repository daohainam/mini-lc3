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
                0x7640,0x1021,0x127F,0x14BE,0x0FF6,0xF025,0x0054,0x0068,0x0069,0x0073,0x0020,0x0069,0x0073,
                0x0020,0x0073,0x006F,0x0020,0x006D,0x0075,0x0063,0x0068,0x0020,0x0066,0x0075,0x006E,0x0021,0x0000
            };

            _machine.Memory.LoadInstructions(instructions);
            _machine.Memory.LoadInstructions([0x1F1F], 0x25); // HALT (TRAP x25)
            _machine.CPU.FetchAndExecuteUntilHalt();

            _machine.CPU.ALU.RegisterFile.R0.Should().Be(0x3014);
            _machine.CPU.ALU.RegisterFile.R1.Should().Be(0x3014);
            _machine.CPU.ALU.RegisterFile.R2.Should().Be(-0x0001);
            _machine.CPU.ALU.RegisterFile.R3.Should().Be(0x54);
            _machine.CPU.ALU.RegisterFile.R4.Should().Be(0x69);
            _machine.CPU.ControlUnit.PC.Should().Be(0x1F1F);
            _machine.CPU.ControlUnit.N.Should().BeTrue();
            _machine.CPU.ControlUnit.Z.Should().BeFalse();
            _machine.CPU.ControlUnit.P.Should().BeFalse();
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
