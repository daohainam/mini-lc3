
using mini_lc3_vm.Components;

namespace mini_lc3_vm;

public interface ILC3Machine
{
    CPU CPU { get; }
    Memory Memory { get; }
    MemoryControlUnit MemoryControlUnit { get; }
    IEnumerable<IAttachable> Devices { get; }

    void Run(CancellationToken cancellationToken);
    void Step();
}
