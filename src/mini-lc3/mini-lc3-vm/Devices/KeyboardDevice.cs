using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Devices
{
    public interface IKeyboardDevice
    {
        char ReadKey();
    }

    public class KeyboardDevice : IKeyboardDevice
    {
        public char ReadKey()
        {
            return Console.ReadKey().KeyChar;
        }

        public static readonly KeyboardDevice Instance = new();
    }
}
