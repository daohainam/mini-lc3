using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Components;

public class ControlUnit
{
    public ushort IR { get; set; }
    public NZP NZP { get; }
}

public struct NZP
{
    public bool N { get; set; } // Negative
    public bool Z { get; set; } // Zero
    public bool P { get; set; } // Positive
}