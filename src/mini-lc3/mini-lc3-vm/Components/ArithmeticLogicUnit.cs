namespace mini_lc3_vm.Components;

public class ArithmeticLogicUnit
{
    public RegisterFile RegisterFile { get; } = new RegisterFile();
}
public struct RegisterFile
{
    public short R0 { get; set; } 
    public short R1 { get; set; }
    public short R2 { get; set; }
    public short R3 { get; set; }
    public short R4 { get; set; }
    public short R5 { get; set; }
    public short R6 { get; set; }
    public short R7 { get; set; }
}
