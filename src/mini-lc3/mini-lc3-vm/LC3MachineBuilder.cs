using mini_lc3_vm.Components;

namespace mini_lc3_vm;

public class LC3MachineBuilder: ILC3MachineBuilder
{
    public ILC3Machine Build()
    {
        var machine = new LC3Machine();
        machine.Memory.LoadProgram(Loop, CPU.DefaultPCAddress);

         return machine;
    }

    public static ILC3MachineBuilder Create(params string[] args)
    {
        return new LC3MachineBuilder();
    }

    /*
        .ORIG x3000
        LD R0, C
        JMP R0
        C .FILL x3000
        .END
    */
    private static readonly short[] Loop = [0x2001, -16384, 0x3000];
}
