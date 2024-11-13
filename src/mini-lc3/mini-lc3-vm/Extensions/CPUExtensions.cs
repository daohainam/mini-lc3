using mini_lc3_vm.Components;

namespace mini_lc3_vm.Extensions;

public static class CPUExtensions
{
    public static void FetchAndExecute(this CPU cpu)
    {
        cpu.Fetch();
        var opcode = cpu.Decode();
        cpu.Execute(opcode);
    }

    public static void FetchAndExecuteUntilHalt(this CPU cpu)
    {
        do
        {
            cpu.FetchAndExecute();
        } while (cpu.ControlUnit.IR != 0xF025);
    }
}
