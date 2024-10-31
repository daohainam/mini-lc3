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
        public void Fetch_IncrementsPCAndLoadsInstruction()
        {
            _cpu.ControlUnit.PC = DefaultPCAddress;
            _memory.LoadInstructions([0x102A]); // ADD R0, R0, #9999
            _cpu.Fetch();
            _cpu.FetchAndExecute();

            _cpu.ControlUnit.IR.Should().Be(0x102A);
            _cpu.ControlUnit.PC.Should().Be(DefaultPCAddress + 1);
            _cpu.ALU.RegisterFile.R0.Should().Be(9999);
        }

        //[Fact]
        //public void Decode_ReturnsCorrectOpcode()
        //{
        //    _cpu.ControlUnit.IR = 0x1000; // Opcode for BR
        //    var opcode = _cpu.Decode();
        //    Assert.AreEqual(Opcodes.BR, opcode);
        //}

        //[Fact]
        //public void Execute_AddInstruction()
        //{
        //    _cpu.ControlUnit.IR = 0x1021; // ADD R0, R0, #1
        //    _cpu.ALU.RegisterFile[0] = 1;
        //    _cpu.Execute(Opcodes.ADD);
        //    Assert.AreEqual(2, _cpu.ALU.RegisterFile[0]);
        //}

        //[Fact]
        //public void Execute_AndInstruction()
        //{
        //    _cpu.ControlUnit.IR = 0x5021; // AND R0, R0, #1
        //    _cpu.ALU.RegisterFile[0] = 3;
        //    _cpu.Execute(Opcodes.AND);
        //    Assert.AreEqual(1, _cpu.ALU.RegisterFile[0]);
        //}

        //[Fact]
        //public void Execute_NotInstruction()
        //{
        //    _cpu.ControlUnit.IR = 0x903F; // NOT R0, R0
        //    _cpu.ALU.RegisterFile[0] = 0;
        //    _cpu.Execute(Opcodes.NOT);
        //    Assert.AreEqual(~0, _cpu.ALU.RegisterFile[0]);
        //}

        //[Fact]
        //public void Execute_LoadInstruction()
        //{
        //    _cpu.ControlUnit.PC = 0x3000;
        //    _cpu.ControlUnit.IR = 0x2001; // LD R0, #1
        //    _memoryControlUnit.Memory[0x3001] = 0x1234;
        //    _cpu.Fetch();
        //    _cpu.Execute(Opcodes.LD);
        //    Assert.AreEqual(0x1234, _cpu.ALU.RegisterFile[0]);
        //}

        //[Fact]
        //public void Execute_StoreInstruction()
        //{
        //    _cpu.ControlUnit.PC = 0x3000;
        //    _cpu.ControlUnit.IR = 0x3001; // ST R0, #1
        //    _cpu.ALU.RegisterFile[0] = 0x1234;
        //    _cpu.Fetch();
        //    _cpu.Execute(Opcodes.ST);
        //    Assert.AreEqual(0x1234, _memoryControlUnit.Memory[0x3001]);
        //}

        //[Fact]
        //public void Execute_BranchInstruction()
        //{
        //    _cpu.ControlUnit.PC = 0x3000;
        //    _cpu.ControlUnit.IR = 0x0E01; // BRnzp #1
        //    _cpu.ControlUnit.NZP = new NZPFlags { N = true, Z = false, P = false };
        //    _cpu.Fetch();
        //    _cpu.Execute(Opcodes.BR);
        //    Assert.AreEqual(0x3001, _cpu.ControlUnit.PC);
        //}

        //[Fact]
        //public void Execute_JumpInstruction()
        //{
        //    _cpu.ControlUnit.IR = 0xC000; // JMP R0
        //    _cpu.ALU.RegisterFile[0] = 0x3000;
        //    _cpu.Execute(Opcodes.JMP);
        //    Assert.AreEqual(0x3000, _cpu.ControlUnit.PC);
        //}

        //[Fact]
        //public void Execute_TrapInstruction()
        //{
        //    _cpu.ControlUnit.IR = 0xF025; // TRAP x25
        //    _cpu.ControlUnit.PC = 0x3000;
        //    _memoryControlUnit.Memory[0x25] = 0x1234;
        //    _cpu.Fetch();
        //    _cpu.Execute(Opcodes.TRAP);
        //    Assert.AreEqual(0x1234, _cpu.ControlUnit.PC);
        //    Assert.AreEqual(0x3000, _cpu.ALU.RegisterFile[7]);
        //}
    }
}
