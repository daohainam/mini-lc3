using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm
{
    internal interface ILC3Memory
    {
        ushort MAR { get; set; }
        short MDR { get; set; }
        void ReadSignal();
        void WriteSignal();
    }
}
