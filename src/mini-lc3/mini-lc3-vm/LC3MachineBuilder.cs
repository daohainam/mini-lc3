using mini_lc3_vm.Components;
using mini_lc3_vm.Devices;
using mini_lc3_vm.ProgramLoaders;

namespace mini_lc3_vm;

public class LC3MachineBuilder: ILC3MachineBuilder
{
    private short[] programCode = [];
    private bool useKeyboard = false;
    private bool useMonitor = false;

    public ILC3Machine Build()
    {
        var machine = new LC3Machine();
        if (useKeyboard)
        {
            machine.AttachDevice(new LC3Keyboard());
        }
        if (useMonitor)
        {
            machine.AttachDevice(new LC3Monitor());
        }

        machine.Memory.LoadInstructions(programCode.Length == 0 ? Loop : programCode, CPU.DefaultPCAddress);

        return machine;
    }

    public static ILC3MachineBuilder Create(params string[] args)
    {
        var builder = new LC3MachineBuilder();

        if (args.Length > 0)
        {
            builder.LoadProgram(args.First());

            bool noKeyboard = false;
            bool noMonitor = false;
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "--no-keyboard")
                {
                    noKeyboard = true;
                }
                else if (args[i] == "--no-monitor")
                {
                    noMonitor = true;
                }
            }

            if (!noKeyboard)
            {
                builder.UseKeyBoard();
            }
            if (!noMonitor)
            {
                builder.UseMonitor();
            }
        }

        return builder;
    }

    private void LoadProgram(string fileName)
    {
        try
        {
            programCode = LoaderFactory.GetLoader(fileName).LoadProgram();
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

    public ILC3MachineBuilder UseMonitor()
    {
        useMonitor = true;
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
