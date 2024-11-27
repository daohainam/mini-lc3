using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm;

public interface ILC3MachineBuilder
{
    ILC3MachineBuilder AddLogging(Action<ILoggingBuilder> configure);
    ILC3Machine Build();
    ILC3MachineBuilder LoadProgram(string fileName);
    ILC3MachineBuilder UseKeyBoard();
    ILC3MachineBuilder UseMonitor();
    ILC3MachineBuilder EnableTimerInterrupt(ushort interval);
}
