using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Components;

public interface IMappedMemory
{
    void OnReadSignal(ushort address, out short data);
    void OnWriteSignal(ushort address, short data);
}
