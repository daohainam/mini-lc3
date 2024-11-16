using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Devices
{
    public interface IGraphicalDisplayDevice
    {
        void Render(Span<short> memory, ushort localtionOnScreen); 
    }
}
