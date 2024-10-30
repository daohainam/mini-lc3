namespace mini_lc3_vm.Components;

public class MemoryControlUnit: ILC3Memory, IMapableMemory
{
    private readonly Memory _memory;
    private readonly Dictionary<MemoryRange, IMappedMemory> _mappedDevices = [];

    public MemoryControlUnit(Memory memory)
    {
        _memory = memory;
    }

    public ushort MAR { get; set; }
    public short MDR { get; set; }

    public void ReadSignal()
    {
        if (MAR >= Memory.MemorySize)
        {
            foreach (var (range, device) in _mappedDevices)
            {
                if (range.Contains(MAR))
                {
                    device.OnReadSignal(MAR, out short v);

                    MDR = v;
                    return;
                }
            }
        }
        else
        {
            MDR = 0; // no mapped device, return 0
        }

        MDR = _memory[MAR];
    }
    public void WriteSignal()
    {
        if (MAR >= Memory.MemorySize) {
            foreach (var (range, device) in _mappedDevices)
            {
                if (range.Contains(MAR))
                {
                    device.OnWriteSignal(MAR, MDR);
                    return;
                }
            }
        }

        _memory[MAR] = MDR;
    }
    public void Map(MemoryRange range, IMappedMemory device)
    {
        if (range.Start < Memory.MemorySize) {
            throw new ArgumentException("Memory range must start within the IO memory space (>= 0xFE00)");
        }

        foreach (var (mappedRange, _) in _mappedDevices)
        {
            if (range.CollidesWith(mappedRange))
            {
                throw new ArgumentException("Memory range collides with existing mapping");
            }
        }

        _mappedDevices.Add(range, device);
    }

}
