namespace mini_lc3_vm.Components;
public class ProgrammableInterruptController
{
    private readonly Queue<byte>[] interruptRequests; // 8 interrupt vectors
    private Lock _lock = new();
    public bool IsMasked { get; set; } = false;

    public ProgrammableInterruptController()
    {
        interruptRequests = new Queue<byte>[8];
        for (int i = 0; i < 8; i++)
        {
            interruptRequests[i] = new Queue<byte>();
        }
    }

    private byte PriorityEncode(byte value)
    {
        // I cannot find a document about the priority encoding scheme used in the LC-3,
        // so I'm assuming it is 8 to 3 encoder where the highest bit set is the priority level
        // https://www.elprocus.com/priority-encoder/

        if (value == 0) return 0;
        for (byte i = 7; i >= 0; i--)
        {
            if ((value & (1 << i)) != 0)
            {
                return i;
            }
        }
        return 0; // should never reach here
    }

    public void RequestInterrupt(byte interrupt)
    {
        lock (_lock)
        {
            byte priority = PriorityEncode(interrupt);
            interruptRequests[priority].Enqueue(interrupt);
        }
    }

    /// <summary>
    /// Acknowledge an interrupt request
    /// </summary>
    /// <param name="currentLevel">priority of the running thread</param>
    /// <param name="vector">interrupt vector (0-255)</param>
    /// <returns>true if there is an interrupt with higher priority waiting</returns>
    public bool AcknowledgeInterrupt(PriorityLevels currentLevel, out byte? vector)
    {
        if (currentLevel == PriorityLevels.Level7) // no interrupts can be acknowledged at level 7
        {
            vector = null;
            return false;
        }

        if (IsMasked) // if the interrupt controller is masked, return false
        {
            vector = null;
            return false;
        }

        lock (_lock)
        {
            for (int i = (int)PriorityLevels.Level7; i > (int)currentLevel; i--)
            {
                if (interruptRequests[i].TryDequeue(out byte interrupt))
                {
                    vector = interrupt;
                    return true;
                }
            }

            vector = null;
            return false;
        }
    }
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
