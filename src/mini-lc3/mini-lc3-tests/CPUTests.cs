namespace mini_lc3_tests
{
    public class CPUTests
    {
        private CPU _cpu;
        private Memory _memory;
        private MemoryControlUnit _memoryControlUnit;

        private const ushort UserSpaceAddress = CPU.UserSpaceAddress;

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
            _cpu.Boot();
            _cpu.ControlUnit.PC = CPU.UserSpaceAddress;
            _memory.LoadInstructions([0x102A]); // ADD R0, R0, #10
            _cpu.FetchAndExecute();

            _cpu.ControlUnit.IR.Should().Be(0x102A);
            _cpu.ControlUnit.PC.Should().Be(UserSpaceAddress + 1);
            _cpu.ALU.RegisterFile.R0.Should().Be(10);
        }

        [Fact]
        public void Execute_AddInstruction()
        {
            _cpu.Boot();
            _cpu.ControlUnit.PC = CPU.UserSpaceAddress;
            _memory.LoadInstructions([0x1021]); // ADD R0, R0, #1
            _cpu.ALU.RegisterFile[0] = 1;
            _cpu.FetchAndExecute();

            _cpu.ALU.RegisterFile[0].Should().Be(2);
        }

        [Fact]
        public void Execute_AndInstruction()
        {
            _cpu.Boot();
            _cpu.ControlUnit.PC = CPU.UserSpaceAddress;
            _memory.LoadInstructions([0x5021]); // AND R0, R0, #1
            _cpu.ALU.RegisterFile[0] = 3;
            _cpu.FetchAndExecute();

            _cpu.ALU.RegisterFile[0].Should().Be(1);
        }

        [Fact]
        public void Execute_NotInstruction()
        {
            _cpu.Boot();
            _cpu.ControlUnit.PC = CPU.UserSpaceAddress;
            _memory.LoadInstructions([0x903F]); // NOT R0, R0
            _cpu.ALU.RegisterFile[0] = 0;
            _cpu.FetchAndExecute();

            _cpu.ALU.RegisterFile[0].Should().Be(~0);
        }

        [Fact]
        public void Execute_LoadInstruction()
        {
            _cpu.ControlUnit.PC = 0x4018;
            _memory.LoadInstructions([0b0010_0101_1010_1111], 0x4018); // LD R2, x1AF
            _memoryControlUnit.Write(0x3FC8, 0x1234, !_cpu.ControlUnit.Privileged);
            _cpu.FetchAndExecute();

            _cpu.ALU.RegisterFile[2].Should().Be(0x1234);
            _cpu.ControlUnit.PC.Should().Be(0x4019);
            _cpu.ControlUnit.N.Should().BeFalse();
            _cpu.ControlUnit.Z.Should().BeFalse();
            _cpu.ControlUnit.P.Should().BeTrue();
        }

        [Fact]
        public void Execute_StoreInstruction()
        {
            _cpu.ControlUnit.PC = 0x4018;
            _memory.LoadInstructions([0b0011_1110_0011_0000], 0x4018); // ST R7, x30
            _cpu.ALU.RegisterFile[7] = 0x123;
            _cpu.FetchAndExecute();

            _memoryControlUnit.Read(0x4049, !_cpu.ControlUnit.Privileged).Should().Be(0x123);
            _cpu.ControlUnit.PC.Should().Be(0x4019);
            _cpu.ControlUnit.N.Should().BeFalse();
            _cpu.ControlUnit.Z.Should().BeFalse();
            _cpu.ControlUnit.P.Should().BeFalse();
        }

        [Fact]
        public void Execute_BranchInstruction_With_ZeroFlag()
        {
            _cpu.ControlUnit.PC = 0x4027;
            _memory.LoadInstructions([0b_0000_0100_1101_1001], 0x4027); // BRnzp #1
            _cpu.ControlUnit.N = false;
            _cpu.ControlUnit.Z = true;
            _cpu.ControlUnit.P = false;
            _cpu.FetchAndExecute();

            _cpu.ControlUnit.PC.Should().Be(0x4101);
        }

        [Fact]
        public void Execute_JumpInstruction()
        {
            _cpu.Boot();
            _cpu.ControlUnit.PC = CPU.UserSpaceAddress;
            _memory.LoadInstructions([0xC000]); // JMP R0
            _cpu.ALU.RegisterFile[0] = 0x3000;
            _cpu.FetchAndExecute();

            _cpu.ControlUnit.PC.Should().Be(0x3000);
        }

        [Fact]
        public void Execute_TrapInstruction()
        {
            _cpu.Boot();
            _cpu.ControlUnit.PC = CPU.UserSpaceAddress;
            _memory.LoadInstructions([0b_1111_0000_0010_0101]);
            _memoryControlUnit.Write(0x25, 0x1234, !_cpu.ControlUnit.Privileged);
            _cpu.FetchAndExecute();

            _cpu.ControlUnit.PC.Should().Be(0x1234);
            _cpu.ALU.RegisterFile[7].Should().Be(0x3001);
        }
    }
}
