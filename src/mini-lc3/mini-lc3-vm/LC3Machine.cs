using mini_lc3_vm.Components;

namespace mini_lc3_vm;

public class LC3Machine: ILC3Machine
{
    private readonly List<IAttachable> devices = [];

    public CPU CPU { get; }
    public Memory Memory { get; }
    public MemoryControlUnit MemoryControlUnit { get; }
    public IEnumerable<IAttachable> Devices => devices;

    public LC3Machine()
    {
        Memory = new();
        MemoryControlUnit = new(Memory);
        CPU = new(MemoryControlUnit);

        Memory.Reset();
    }

    public void Run(CancellationToken cancellationToken)
    {
        CPU.Boot();
        CPU.Run(cancellationToken);
    }

    public void AttachDevice(IAttachable device)
    {
        device.Attach(this);
        devices.Add(device);
    }
}
