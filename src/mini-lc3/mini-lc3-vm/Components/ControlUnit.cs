using System.Diagnostics;

namespace mini_lc3_vm.Components;

[DebuggerDisplay("PC = {PC}, IR = {IR}, PSR = {PSR}, MCR = {MCR}, MCC = {MCR}, TimerCycleInterval = {TimerCycleInterval}")]
public class ControlUnit
{
    public ushort IR { get; set; } // Instruction Register
    public ushort PC { get; set; } // Program Counter
    public ushort PSR { get; set; } // Processor Status Register
    public ushort MCR { get; set; } = 0b_1000_0000_0000_0000; // clock enabled, timer interrupt disabled, timer cycle interval = 0
    public ushort MCC { get; set; } = 0b_0000_0000_0000_0000; // timer cycle count = 0
    public bool P
    {
        get => (PSR & 0x1) == 0x1;
        set => PSR = (ushort)(value ? PSR | 0x1 : PSR & ~0x1);
    }
    public bool Z
    {
        get => (PSR & 0x2) == 0x2;
        set => PSR = (ushort)(value ? PSR | 0x2 : PSR & ~0x2);
    }
    public bool N
    {
        get => (PSR & 0x4) == 0x4;
        set => PSR = (ushort)(value ? PSR | 0x4 : PSR & ~0x4);
    }
    public bool Privileged
    {
        get => (PSR & 0x8000) == 0x8000;
        set => PSR = (ushort)(value ? PSR | 0x8000 : PSR & ~0x8000);
    }
    public ushort Priority
    {
        get => (ushort)((PSR & 0x700) >> 8);
        set => PSR = (ushort)((PSR & ~0x700) | ((value & 0x07) << 8));
    }
    public bool ClockEnable
    {
        get => (MCR & 0x8000) == 0x8000;
        set => MCR = (ushort)(value ? MCR | 0x8000 : MCR & ~0x8000);
    }
    public bool TimerInterruptEnable
    {
        get => (MCR & 0x4000) == 0x100;
        set => MCR = (ushort)(value ? MCR | 0x4000 : MCR & ~0x4000);
    }
    public ushort TimerCycleInterval
    {
        get => (ushort)(MCR & 0x3FFF);
        set => MCR = (ushort)((MCR & 0xC000) | (value & 0x3FFF));
    }
}
