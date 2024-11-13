using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.ExecuteableFile;

internal class ExecutableImage: IExecutableImage
{
    public ushort LoadAddress { get; }
    public ushort[] Instructions { get; }

    public ExecutableImage(ushort loadAddress, ushort[] instructions)
    {
        LoadAddress = loadAddress;
        Instructions = instructions;
    }

    public ExecutableImage(ushort loadAddress, int instructionBufferLength)
    {
        LoadAddress = loadAddress;
        Instructions = new ushort[instructionBufferLength];
    }

    public override string ToString()
    {
        return $"Load Address: {LoadAddress}, Instructions: {string.Join(", ", Instructions.SelectMany(ins => ins.ToString("XXXX")))}";
    }
}
