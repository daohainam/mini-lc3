﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using mini_lc3_vm.Components;

namespace mini_lc3_vm;

public class LC3Machine: ILC3Machine
{
    public string Name { get; set; }

    private readonly List<IAttachable> devices = [];

    public CPU CPU { get; }
    public Memory Memory { get; }
    public MemoryControlUnit MemoryControlUnit { get; }
    public IEnumerable<IAttachable> Devices => devices;

    public LC3Machine(ILoggerFactory? loggingFactory): this("LC3", loggingFactory)
    {
    }
    public LC3Machine(string name, ILoggerFactory? loggingFactory = null)
    {
        Name = name;

        Memory = new(loggingFactory != null ? loggingFactory.CreateLogger<Memory>() : NullLogger<Memory>.Instance);
        MemoryControlUnit = new(Memory, loggingFactory != null ? loggingFactory.CreateLogger<MemoryControlUnit>() : NullLogger<MemoryControlUnit>.Instance);
        CPU = new(MemoryControlUnit, loggingFactory != null ? loggingFactory.CreateLogger(Name + ".CPU") : NullLogger<CPU>.Instance);

        AttachDevice(CPU); // map MCR register 

        Memory.Reset();
        CPU.Boot();
    }

    public void Run(CancellationToken cancellationToken)
    {
        CPU.Run(cancellationToken);
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
