using mini_lc3_vm;

var builder = LC3MachineBuilder.Create(args);
var machine = builder.Build();

var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;

Console.CancelKeyPress += (sender, e) =>
{
    if (e.SpecialKey == ConsoleSpecialKey.ControlC)
    {
        Console.WriteLine("Shutting down...");
        
        cancellationTokenSource.Cancel();
        e.Cancel = true;
    }
};

Console.WriteLine("Press Ctrl-C to shutdown machine...");
machine.Run(cancellationToken);

