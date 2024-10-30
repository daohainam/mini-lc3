using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.ProgramLoaders
{
    public class LoaderFactory
    {
        public static IProgramLoader GetLoader(string filename)
        {
            if (filename.EndsWith(".bin", StringComparison.OrdinalIgnoreCase))
            {
                return new BinProgramLoader(filename);
            }
            else if (filename.EndsWith(".hex", StringComparison.OrdinalIgnoreCase))
            {
                return new HexProgramLoader(filename);
            }
            else
            {
                throw new ArgumentException("Unknown file type");
            }
        }
    }
}
