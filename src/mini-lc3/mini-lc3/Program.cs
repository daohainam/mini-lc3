﻿using Microsoft.Extensions.Logging;
using mini_lc3;
using mini_lc3_vm;

if (!args.Contains("--no-logo"))
{ 
    Logo.Print();
}

var builder = LC3MachineBuilder.Create(args);
builder.AddLogging(builder => builder.AddSimpleConsole(options =>
{
    options.IncludeScopes = false;
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
    options.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled;
}).SetMinimumLevel(LogLevel.Information));
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
Console.WriteLine();
machine.Run(cancellationToken);

