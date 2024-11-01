using mini_lc3_vm.Devices;
using System.Collections.Concurrent;
using System.Text;

namespace mini_lc3_tests.Devices
{
    internal class MemoryConsoleDevice : IKeyboardDevice, IMonitorDevice
    {
        private ConcurrentQueue<byte> Input { get; } = new();
        private SemaphoreSlim semaphore = new(0, 1000);
        private ConcurrentQueue<byte> Output { get; } = new();

        public byte Read()
        {
            semaphore.Wait();
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
                semaphore.Release(); 
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
}
