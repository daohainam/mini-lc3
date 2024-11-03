using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.ExecuteableFile
{
    public class InvalidExecutableImageException : Exception
    {
        public InvalidExecutableImageException(string message) : base(message)
        {
        }
    }
}
