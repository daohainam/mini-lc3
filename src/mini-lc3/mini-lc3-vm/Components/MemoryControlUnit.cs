namespace mini_lc3_vm.Components;

public class MemoryControlUnit: ILC3MemoryControlUnit, IMapableMemory
{
    private readonly Memory _memory;
    private ushort lowestDeviceMappedAddress;
    private readonly Dictionary<MemoryRange, IMappedMemory> _mappedDevices = [];
    private ILogger<MemoryControlUnit> logger;

    public MemoryControlUnit(Memory memory, ILogger<MemoryControlUnit> logger)
    {
        _memory = memory;
        lowestDeviceMappedAddress = Memory.MemorySize;
        this.logger = logger;
    }

    public MemoryControlUnit(Memory memory) : this(memory, NullLogger<MemoryControlUnit>.Instance)
    {
    }

    public ushort MAR { get; set; }
    public short MDR { get; set; }
    public ushort MPR { get; set; } = 0xFFFF; // memory protection register, https://acg.cis.upenn.edu/milom/cse240-Fall05/handouts/Ch09-a.pdf

    public void ReadSignal(bool isUserMode)
    {
        EnsureProtectionLevel(isUserMode);

        if (MAR >= lowestDeviceMappedAddress)
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

            MDR = 0; // no mapped device, return 0
        }

        MDR = _memory[MAR];
    }

    public void WriteSignal(bool isUserMode)
    {
        EnsureProtectionLevel(isUserMode);

        if (MAR >= lowestDeviceMappedAddress) {
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
        if (range.Start < 0x3000) {
            throw new ArgumentException("Memory range must not start within the kernel space (>= 0x3000)");
        }

        foreach (var (mappedRange, _) in _mappedDevices)
        {
            if (range.CollidesWith(mappedRange))
            {
                throw new ArgumentException("Memory range collides with existing mapping");
            }
        }

        if (lowestDeviceMappedAddress > range.Start)
        {
            lowestDeviceMappedAddress = range.Start;
        }

        _mappedDevices.Add(range, device);
    }

    private void EnsureProtectionLevel(bool isUserMode)
    {
        if (isUserMode)
        {
            var block = (MAR >> 12) & 0xF;
            if (((MPR >> block) & 1) == 0)
            {
                throw new MemoryAccessViolationException(MAR);
            }
        }
    }



}
