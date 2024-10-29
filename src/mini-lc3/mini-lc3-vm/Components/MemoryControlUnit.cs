namespace mini_lc3_vm.Components;

public class MemoryControlUnit: ILC3Memory
{
    private readonly Memory _memory;

    public MemoryControlUnit(Memory memory)
    {
        _memory = memory;
    }

    public ushort MAR { get; set; }
    public short MDR { get; set; }
    public void ReadSignal()
    {
        MDR = _memory[MAR];
    }
    public void WriteSignal()
    {
        _memory[MAR] = MDR;
    }
}
