using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.ExecuteableFile
{
    public interface IExecutableImage
    {
        ushort LoadAddress { get; }
        ushort[] Instructions { get; }
    }
}
