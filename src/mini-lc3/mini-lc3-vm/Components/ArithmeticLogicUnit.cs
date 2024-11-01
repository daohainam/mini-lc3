using System.Diagnostics;

namespace mini_lc3_vm.Components;

public class ArithmeticLogicUnit
{
    public RegisterFile RegisterFile { get; } = new RegisterFile();
}

[DebuggerDisplay("R0 = {R0}, R1 = {R1}, R2 = {R2}, R3 = {R3}, R4 = {R4}, R5 = {R5}, R6 = {R6}, R7 = {R7}")]
public class RegisterFile
{
    private readonly short[] _registers = new short[8];

    public short R0
    {
        get => _registers[0];
        set => _registers[0] = value;
    }
    public short R1
    {
        get => _registers[1];
        set => _registers[1] = value;
    }
    public short R2
    {
        get => _registers[2];
        set => _registers[2] = value;
    }
    public short R3
    {
        get => _registers[3];
        set => _registers[3] = value;
    }
    public short R4
    {
        get => _registers[4];
        set => _registers[4] = value;
    }
    public short R5
    {
        get => _registers[5];
        set => _registers[5] = value;
    }
    public short R6
    {
        get => _registers[6];
        set => _registers[6] = value;
    }
    public short R7
    {
        get => _registers[7];
        set => _registers[7] = value;
    }

    public short this[int index]
    {
        get => _registers[index];
        set => _registers[index] = value;
    }
}
