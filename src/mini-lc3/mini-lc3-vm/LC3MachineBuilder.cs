using mini_lc3_vm.Components;
using mini_lc3_vm.Devices;

namespace mini_lc3_vm;

public class LC3MachineBuilder: ILC3MachineBuilder
{
    private short[] programCode = [];
    private bool useKeyboard = false;

    public ILC3Machine Build()
    {
        var machine = new LC3Machine();
        if (useKeyboard)
        {
            machine.AttachDevice(new Keyboard());
        }

        machine.Memory.LoadProgram(programCode.Length == 0 ? Loop : programCode, CPU.DefaultPCAddress);

        return machine;
    }

    public static ILC3MachineBuilder Create(params string[] args)
    {
        var builder = new LC3MachineBuilder();

        if (args.Length > 0)
        {
            builder.LoadProgram(args.First());

            if (!args.Contains("--no-keyboard"))
            {
                builder.UseKeyBoard();
            }
        }

        return builder;
    }

    private void LoadProgram(string objectFileName)
    {
        try
        {
            programCode = File.ReadAllBytes(objectFileName)
                              .Select((b, i) => new { b, i })
                              .GroupBy(x => x.i / 2)
                              .Select(g => (short)(g.First().b | (g.Last().b << 8)))
                              .ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading program: {ex.Message}");
            throw;
        }
    }

    public ILC3MachineBuilder UseKeyBoard()
    {
        useKeyboard = true;
        return this;
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
