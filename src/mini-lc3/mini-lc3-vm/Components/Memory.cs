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
        public const int MemorySize = 0x10000;

        private readonly short[] _memory = new short[MemorySize];

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

        public void LoadProgram(short[] program, int address)
        {
            if (program.Length + address > MemorySize)
            {
                throw new ArgumentException("Program is too large to fit in memory");
            }

            Array.Copy(program, 0, _memory, address, program.Length);
        }

        public void Reset()
        {
            Array.Clear(_memory, 0, MemorySize);
        }
    }
}
