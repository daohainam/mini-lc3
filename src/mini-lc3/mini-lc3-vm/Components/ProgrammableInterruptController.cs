namespace mini_lc3_vm.Components;

public class ProgrammableInterruptController(MemoryControlUnit memoryControlUnit)
{
    private readonly List<DeviceIRQRegister>[] deviceIRQRegisters = new List<DeviceIRQRegister>[8];
    private readonly MemoryControlUnit memoryControlUnit = memoryControlUnit;

    public void RegisterDeviceIRQRegister(ushort registerAddress, byte interruptVector, PriorityLevels priorityLevel)
    {
        deviceIRQRegisters[(byte)priorityLevel].Add(new DeviceIRQRegister(registerAddress, interruptVector));
    }
    public void UnregisterDeviceIRQRegister(ushort registerAddress, byte interruptVector, PriorityLevels priorityLevel)
    {
        deviceIRQRegisters[(byte)priorityLevel].RemoveAll(x => x.RegisterAddress == registerAddress && x.InterruptVector == interruptVector);
    }

    public bool TryGetNextSignal(ushort currentPriorityLevel, out InterruptSignal? signal)
    {
        for (byte pl = 7; pl > currentPriorityLevel; pl--) // start from highest priority level
        {
            if (deviceIRQRegisters[pl].Count > 0)
            {
                foreach (var r in deviceIRQRegisters[pl])
                {
                    memoryControlUnit.MAR = r.RegisterAddress;
                    memoryControlUnit.ReadSignal(false); // PIC can always read memory
                    if ((memoryControlUnit.MDR & 0x04000) == 0x04000) // test bit 14
                    {
                        // turn it off
                        memoryControlUnit.MDR = (short)(memoryControlUnit.MDR & 0xBFFF);
                        memoryControlUnit.WriteSignal(true);

                        signal = new(r.InterruptVector, (PriorityLevels)pl);
                        return true;
                    }
                }
            }
        }

        signal = null;
        return false;
    }

    private record DeviceIRQRegister(ushort RegisterAddress, byte InterruptVector);
    
    public record InterruptSignal(byte interruptVector, PriorityLevels priorityLevel);
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
