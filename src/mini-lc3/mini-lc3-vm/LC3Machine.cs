using mini_lc3_vm.Components;

namespace mini_lc3_vm;

public class LC3Machine: ILC3Machine
{
    public CPU CPU { get; }
    public Memory Memory { get; }
    public MemoryControlUnit MemoryControlUnit { get; }

    public LC3Machine()
    {
        Memory = new();
        MemoryControlUnit = new(Memory);
        CPU = new(MemoryControlUnit);
    }
}
