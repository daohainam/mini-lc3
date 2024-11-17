using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Debugging
{
    public interface IDebuggable
    {
        void AddBreakPoint(BreakPoint breakPoint);
        bool RemoveBreakPoint(ushort address);
    }
}
