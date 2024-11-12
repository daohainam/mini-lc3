using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using mini_lc3_vm.Components;
using mini_lc3_vm.Devices;
using mini_lc3_vm.ExecuteableFile;
using mini_lc3_vm.OS;
using mini_lc3_vm.ProgramLoaders;
using System.Linq;

namespace mini_lc3_vm;

public class LC3MachineBuilder: ILC3MachineBuilder
{
    private bool useKeyboard = false;
    private bool useMonitor = false;
    private ushort[] osImage = EmbeddedOS.OsImage;
    private IExecutableImage? executableImage;
    private ServiceCollection services = new();

    public ILC3Machine Build()
    {
        var serviceProvider = services.BuildServiceProvider();
        var loggingFactory = serviceProvider.GetService<ILoggerFactory>();
        if (loggingFactory is null)
        {
            services.AddLogging(builder => builder.AddConsole());
        }

        var machine = new LC3Machine(loggingFactory);
        if (useKeyboard)
        {
            machine.AttachDevice(new LC3Keyboard());
        }
        if (useMonitor)
        {
            machine.AttachDevice(new LC3Monitor());
        }

        machine.Memory.LoadInstructions(osImage, 0);

        executableImage ??= new ExecutableImage(0x3000, Loop);
        machine.Memory.LoadInstructions(executableImage.Instructions, executableImage.LoadAddress);


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
                else if (args[i].StartsWith("--os="))
                {
                    if (!LoadOSImage(args[i][5..], out ushort[] image))
                    {
                        throw new ArgumentException("Invalid OS image");
                    }
                    else
                    {
                        builder.osImage = image;
                    }
                }
                else
                {
                    Console.WriteLine($"Unknown argument: {args[i]}");
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

    private static bool LoadOSImage(string fileName, out ushort[] image)
    {
        try
        {
            image = LoaderFactory.GetLoader(fileName).LoadExecutableFile().Instructions;
            return true;
        }
        catch
        {
            image = [];
            return false;
        }
    }

    public ILC3MachineBuilder AddLogging(Action<ILoggingBuilder> configure)
    {
        services.AddLogging(configure);
        return this;
    }

    public ILC3MachineBuilder LoadProgram(string fileName)
    {
        try
        {
            this.executableImage = LoaderFactory.GetLoader(fileName).LoadExecutableFile();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading program: {ex.Message}");
            throw;
        }

        return this;
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
    private static readonly ushort[] Loop = [0x2001, 0xC000, 0x3000];
}
