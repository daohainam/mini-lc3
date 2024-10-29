namespace mini_lc3_vm.Components;

public class CPU
{
    public ArithmeticLogicUnit ALU { get; } 
    public ControlUnit ControlUnit { get; }
    public MemoryControlUnit MemoryControlUnit { get; }

    public CPU(MemoryControlUnit memoryControlUnit)
    {
        ALU = new();
        ControlUnit = new();
        MemoryControlUnit = memoryControlUnit;
    }

    public void Fetch()
    {
        MemoryControlUnit.MAR = ControlUnit.PC++;
        MemoryControlUnit.ReadSignal();
        ControlUnit.IR = (ushort)MemoryControlUnit.MDR;
    }
}
