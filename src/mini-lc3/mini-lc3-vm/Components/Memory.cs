using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Components
{
    public class Memory: IEnumerable<short>
    {
        public const int IOPageSize = 0x200;
        public const int MemorySize = 0x10000 - IOPageSize;

        private readonly short[] _memory = new short[MemorySize];
        private ILogger<Memory> logger;

        public Memory() : this(NullLogger<Memory>.Instance)
        {
        }

        public Memory(ILogger<Memory> logger)
        {
            this.logger = logger;
        }

        public IEnumerator<short> GetEnumerator()
        {
            return ((IEnumerable<short>)_memory).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _memory.GetEnumerator();
        }

        public short this[int index]
        {
            get => _memory[index];
            set => _memory[index] = value;
        }

        public void LoadInstructions(ushort[] instructions, int address = CPU.UserSpaceAddress)
        {
            ArgumentNullException.ThrowIfNull(instructions, nameof(instructions));
            ArgumentOutOfRangeException.ThrowIfLessThan(address, 0, nameof(address));

            if (instructions.Length == 0)
            {
                return;
            }

            if (instructions.Length + address > MemorySize)
            {
                throw new ArgumentException("Program is too large to fit in memory");
            }

            Array.Copy(instructions, 0, _memory, address, instructions.Length);
        }

        public void Reset()
        {
            Array.Clear(_memory, 0, MemorySize);
        }
    }
}
