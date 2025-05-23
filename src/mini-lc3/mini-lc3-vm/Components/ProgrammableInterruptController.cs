﻿namespace mini_lc3_vm.Components;

public class ProgrammableInterruptController
{
    private readonly List<DeviceIRQRegister>[] deviceIRQRegisters;
    private readonly MemoryControlUnit memoryControlUnit;

    public ProgrammableInterruptController(MemoryControlUnit memoryControlUnit)
    {
        this.memoryControlUnit = memoryControlUnit;

        deviceIRQRegisters = new List<DeviceIRQRegister>[8];
        for (int i = 0; i < 8; i++)
        {
            deviceIRQRegisters[i] = [];
        }
    }

    public void RegisterDeviceIRQRegister(ushort registerAddress, byte interruptVector, PriorityLevels priorityLevel, byte cpu)
    {
        deviceIRQRegisters[(byte)priorityLevel].Add(new DeviceIRQRegister(registerAddress, interruptVector, cpu));
    }
    public void UnregisterDeviceIRQRegister(ushort registerAddress, byte interruptVector, PriorityLevels priorityLevel)
    {
        deviceIRQRegisters[(byte)priorityLevel].RemoveAll(x => x.RegisterAddress == registerAddress && x.InterruptVector == interruptVector);
    }

    public bool TryGetNextSignal(byte cpu, ushort currentPriorityLevel, out InterruptSignal? signal)
    {
        for (byte pl = 7; pl > currentPriorityLevel; pl--) // start from highest priority level
        {
            if (deviceIRQRegisters[pl].Count > 0)
            {
                foreach (var r in deviceIRQRegisters[pl])
                {
                    if (r.cpu != cpu)
                    {
                        continue;
                    }

                    memoryControlUnit.MAR = r.RegisterAddress;
                    memoryControlUnit.ReadSignal(false); // PIC can always read memory
                    if ((memoryControlUnit.MDR & 0x04000) == 0x04000) // test bit 14
                    {
                        // turn it off
                        memoryControlUnit.MDR = (short)(memoryControlUnit.MDR & 0xBFFF);
                        memoryControlUnit.WriteSignal(false);

                        signal = new(r.InterruptVector, (PriorityLevels)pl, r.cpu);
                        return true;
                    }
                }
            }
        }

        signal = null;
        return false;
    }

    private record DeviceIRQRegister(ushort RegisterAddress, byte InterruptVector, byte cpu);
    
    public record InterruptSignal(byte interruptVector, PriorityLevels priorityLevel, byte cpu);
}

public enum PriorityLevels : byte
{
    Level0 = 0,
    Level1 = 1,
    Level2 = 2,
    Level3 = 3,
    Level4 = 4,
    Level5 = 5,
    Level6 = 6,
    Level7 = 7
}
