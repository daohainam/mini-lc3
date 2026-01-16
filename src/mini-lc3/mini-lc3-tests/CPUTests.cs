using mini_lc3_vm.Components;

namespace mini_lc3_tests;

public class CPUTests
{
    private CPU _cpu;
    private Memory _memory;
    private MemoryControlUnit _memoryControlUnit;
    private ProgrammableInterruptController _pic;

    private const ushort UserSpaceAddress = CPU.UserSpaceAddress;

    public CPUTests()
    {
        _memory = new Memory();
        _memory.Reset();
        _memoryControlUnit = new MemoryControlUnit(_memory);
        _pic = new ProgrammableInterruptController(_memoryControlUnit);

        _cpu = new CPU(_memoryControlUnit, _pic, 0);
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

    [Fact]
    public void Execute_Interrupt() { 
        _cpu.Boot();
        _cpu.ControlUnit.PC = CPU.UserSpaceAddress;
        _cpu.ControlUnit.PSR = 0b_0000_0111_0000_0000; // enable interrupts
        _cpu.ALU.RegisterFile[6] = 0x4000; // USP
        _cpu.SavedSSP = 0x3000;

        _memory.LoadInstructions([0x0300], 0x0108); // interrupt vector 8

        _cpu.CallInterrupt(8, PriorityLevels.Level4);

        _cpu.ControlUnit.PC.Should().Be(0x0300);
        _cpu.ControlUnit.Priority.Should().Be(4);
        _cpu.SavedUSP.Should().Be(0x4000);
        _cpu.ALU.RegisterFile[6].Should().Be(0x3000 - 2); // SSP
        _memory[0x3000 - 1].Should().Be(0b_0000_0111_0000_0000);
        _memory[0x3000 - 2].Should().Be((short)CPU.UserSpaceAddress);
    }

    [Fact]
    public void Execute_LEA_SetsCorrectNZPFlags()
    {
        _cpu.Boot();
        _cpu.ControlUnit.PC = 0x3000;
        // LEA R0, #-1 (should load address 0x3000 which is positive)
        _memory.LoadInstructions([0b_1110_000_111111111], 0x3000); // LEA R0, #-1
        _cpu.FetchAndExecute();

        _cpu.ALU.RegisterFile[0].Should().Be(0x3000);
        _cpu.ControlUnit.P.Should().BeTrue();
        _cpu.ControlUnit.Z.Should().BeFalse();
        _cpu.ControlUnit.N.Should().BeFalse();
    }

    [Fact]
    public void Execute_JSR_WithSignExtension()
    {
        _cpu.Boot();
        _cpu.ControlUnit.PC = 0x3100;
        // JSR with offset -256 (0x700 with sign bit set, should extend to 0xF700)
        _memory.LoadInstructions([0b_0100_1_11100000000], 0x3100); // JSR #-256
        _cpu.FetchAndExecute();

        // PC should be 0x3100 + 1 (after fetch) + (-256) = 0x3101 - 256 = 0x3001
        _cpu.ControlUnit.PC.Should().Be(0x3001);
        _cpu.ALU.RegisterFile[7].Should().Be(0x3101);
    }

    [Fact]
    public void Execute_JSRR_SavesReturnAddress()
    {
        _cpu.Boot();
        _cpu.ControlUnit.PC = 0x3100;
        _cpu.ALU.RegisterFile[2] = 0x4000;
        // JSRR R2 (bit 11 = 0 for JSRR)
        _memory.LoadInstructions([0b_0100_0_00_010_000000], 0x3100); // JSRR R2
        _cpu.FetchAndExecute();

        _cpu.ControlUnit.PC.Should().Be(0x4000);
        _cpu.ALU.RegisterFile[7].Should().Be(0x3101);
    }

    [Fact]
    public void TimerInterruptEnable_GetterReturnsCorrectValue()
    {
        _cpu.ControlUnit.MCR = 0x4000;
        _cpu.ControlUnit.TimerInterruptEnable.Should().BeTrue();

        _cpu.ControlUnit.MCR = 0x0000;
        _cpu.ControlUnit.TimerInterruptEnable.Should().BeFalse();
    }
}
