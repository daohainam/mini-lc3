using mini_lc3_vm.Devices;
using System.Collections.Concurrent;
using System.Text;

namespace mini_lc3_tests.Devices;

internal class MemoryConsoleDevice : IKeyboardDevice, IMonitorDevice
{
    private ConcurrentQueue<byte> Input { get; } = new();
    private ConcurrentQueue<byte> Output { get; } = new();

    bool IMonitorDevice.Ready => true;

    bool IKeyboardDevice.Ready => !Input.IsEmpty;

    public byte Read()
    {
        if (Input.IsEmpty)
        {
            throw new InvalidOperationException("No input available");
        }
        
        if (!Input.TryDequeue(out var c))
        {
            throw new InvalidOperationException("Failed to read input");
        }

        return c;
    }

    public void Write(byte c)
    {
        Output.Enqueue(c);
    }

    public void SimulateKeyboardTyping(string s)
    {
        ArgumentNullException.ThrowIfNull(s);

        if (s.Length == 0)
        {
            return;
        }

        foreach (var c in s)
        {
            Input.Enqueue((byte)c);
        }
    }

    public string GetOutput()
    {
        var sb = new StringBuilder();
        while (!Output.IsEmpty)
        {
            if (!Output.TryDequeue(out var c))
            {
                throw new InvalidOperationException("Failed to read output");
            }

            sb.Append((char)c);
        }

        return sb.ToString();
    }
}
