using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Exceptions;

public class MemoryAccessViolationException: Exception
{
    public ushort Address { get; }
    public MemoryAccessViolationException(ushort address)
    {
        Address = address;
    }
}
