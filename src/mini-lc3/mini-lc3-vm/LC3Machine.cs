namespace mini_lc3_vm;

public partial class LC3Machine: ILC3Machine, IDebuggable
{
    public string Name { get; set; }

    private readonly List<IAttachable> devices = [];

    public CPU CPU { get; }
    public Memory Memory { get; }
    public MemoryControlUnit MemoryControlUnit { get; }
    public ProgrammableInterruptController PIC { get; }
    public IEnumerable<IAttachable> Devices => devices;

    public LC3Machine(ILoggerFactory? loggingFactory = null): this("LC3", loggingFactory)
    {
    }
    public LC3Machine(string name, ILoggerFactory? loggingFactory = null)
    {
        Name = name;

        Memory = new(loggingFactory != null ? loggingFactory.CreateLogger<Memory>() : NullLogger<Memory>.Instance);
        MemoryControlUnit = new(Memory, loggingFactory != null ? loggingFactory.CreateLogger<MemoryControlUnit>() : NullLogger<MemoryControlUnit>.Instance);
        PIC = new(MemoryControlUnit);

        CPU = new(MemoryControlUnit, PIC, loggingFactory != null ? loggingFactory.CreateLogger(Name + ".CPU") : NullLogger<CPU>.Instance);

        AttachDevice(CPU); // map MCR register 

        Memory.Reset();
        CPU.Boot();
    }

    public void Run(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && CPU.ControlUnit.ClockEnable)
        {
            Step();

            if (breakPoints.Count > 0)
            {
                // build machine state for debugging
                var state = new MachineState(
                    CPU.ControlUnit.PC,
                    CPU.ControlUnit.IR,
                    CPU.ControlUnit.PSR,
                    CPU.ALU.RegisterFile,
                    Memory.GetReadOnlyMemory()
                    );

                // check if we hit a breakpoint and stop execution
                if (breakPoints.TryGetValue(CPU.ControlUnit.PC, out var breakPoint))
                {
                    if (breakPoint.Condition(state))
                    {
                        break;
                    }
                }
            }
        }
    }

    public void Step()
    {
        CPU.Step();
    }

    public void AttachDevice(IAttachable device)
    {
        device.Attach(this);
        devices.Add(device);
    }
}
