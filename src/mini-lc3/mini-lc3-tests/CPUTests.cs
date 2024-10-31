using FluentAssertions;
using mini_lc3_vm.Components;
using mini_lc3_vm.Extensions;

namespace mini_lc3_tests
{
    public class CPUTests
    {
        private CPU _cpu;
        private Memory _memory;
        private MemoryControlUnit _memoryControlUnit;

        private const ushort DefaultPCAddress = CPU.DefaultPCAddress;

        public CPUTests()
        {
            _memory = new Memory();
            _memory.Reset();
            _memoryControlUnit = new MemoryControlUnit(_memory);
            _cpu = new CPU(_memoryControlUnit);
        }

        [Fact]
        public void Boot_SetsPCToDefaultAddress()
        {
            _cpu.Boot();
            _cpu.ControlUnit.PC.Should().Be(CPU.DefaultPCAddress);
        }

        [Fact]
        public void Fetch_And_Execute_AddInstruction()
        {
            _cpu.ControlUnit.PC = DefaultPCAddress;
            _memory.LoadInstructions([0x102A]); // ADD R0, R0, #10
            _cpu.FetchAndExecute();

            _cpu.ControlUnit.IR.Should().Be(0x102A);
            _cpu.ControlUnit.PC.Should().Be(DefaultPCAddress + 1);
            _cpu.ALU.RegisterFile.R0.Should().Be(10);
        }

        [Fact]
        public void Execute_AddInstruction()
        {
            _cpu.ControlUnit.IR = 0x1021; // ADD R0, R0, #1
            _cpu.ALU.RegisterFile[0] = 1;
            _cpu.Execute(Opcodes.ADD);
            _cpu.ALU.RegisterFile[0].Should().Be(2);
        }

        [Fact]
        public void Execute_AndInstruction()
        {
            _cpu.ControlUnit.IR = 0x5021; // AND R0, R0, #1
            _cpu.ALU.RegisterFile[0] = 3;
            _cpu.Execute(Opcodes.AND);
            _cpu.ALU.RegisterFile[0].Should().Be(1);
        }

        [Fact]
        public void Execute_NotInstruction()
        {
            _cpu.ControlUnit.IR = 0x903F; // NOT R0, R0
            _cpu.ALU.RegisterFile[0] = 0;
            _cpu.Execute(Opcodes.NOT);
            _cpu.ALU.RegisterFile[0].Should().Be(~0);
        }

        [Fact]
        public void Execute_LoadInstruction()
        {
            _cpu.ControlUnit.PC = 0x3000;
            _cpu.ControlUnit.IR = 0x2001; // LD R0, #1
            _memoryControlUnit.Write(0x3001, 0x1234);
            _cpu.Fetch();
            _cpu.Execute(Opcodes.LD);
            _cpu.ALU.RegisterFile[0].Should().Be(0x1234);
        }

        [Fact]
        public void Execute_StoreInstruction()
        {
            _cpu.ControlUnit.PC = 0x3000;
            _cpu.ControlUnit.IR = 0x3001; // ST R0, #1
            _cpu.ALU.RegisterFile[0] = 0x1234;
            _cpu.Fetch();
            _cpu.Execute(Opcodes.ST);
            _memoryControlUnit.Read(0x3001).Should().Be(0x1234);
        }

        [Fact]
        public void Execute_BranchInstruction()
        {
            _cpu.ControlUnit.PC = 0x3000;
            _cpu.ControlUnit.IR = 0x0E01; // BRnzp #1
            _cpu.ControlUnit.NZP.N = true;
            _cpu.ControlUnit.NZP.Z = false;
            _cpu.ControlUnit.NZP.P = false;
            _cpu.Fetch();
            _cpu.Execute(Opcodes.BR);
            _cpu.ControlUnit.PC.Should().Be(0x3001);
        }

        [Fact]
        public void Execute_JumpInstruction()
        {
            _cpu.ControlUnit.IR = 0xC000; // JMP R0
            _cpu.ALU.RegisterFile[0] = 0x3000;
            _cpu.Execute(Opcodes.JMP);
            _cpu.ControlUnit.PC.Should().Be(0x3000);
        }

        [Fact]
        public void Execute_TrapInstruction()
        {
            _cpu.ControlUnit.IR = 0xF025; // TRAP x25
            _cpu.ControlUnit.PC = 0x3000;
            _memoryControlUnit.Write(0x25, 0x1234);
            _cpu.Fetch();
            _cpu.Execute(Opcodes.TRAP);
            _cpu.ControlUnit.PC.Should().Be(0x1234);
            _cpu.ALU.RegisterFile[7].Should().Be(0x3000);
        }
    }
}
