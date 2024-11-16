using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using mini_lc3_vm.Exceptions;
using System.Buffers.Text;

namespace mini_lc3_vm.Components;

public class CPU: IAttachable, IMappedMemory
{
    public const ushort DefaultPCAddress = 0x0200; // OS's first address
    public const ushort UserSpaceAddress = 0x3000;

    public const ushort TMR_ADDRESS = 0xFE08;
    public const ushort TMI_ADDRESS = 0xFE0A;
    public const ushort MPR_ADDRESS = 0xFE12;
    public const ushort MCR_ADDRESS = 0xFFFE;
    public const ushort MCC_ADDRESS = 0xFFFF;

    public const byte TIMER_INTERRUPT = 2;

    public ArithmeticLogicUnit ALU { get; } 
    public ControlUnit ControlUnit { get; }
    public MemoryControlUnit MemoryControlUnit { get; }
    public ProgrammableInterruptController PIC { get; }

    private short SavedSSP { get; set; } // saved system stack pointer
    private short SavedUSP { get; set; } // saved user stack pointer

    public bool IsAttached { get; private set; } = false;

    private readonly ILogger logger;

    public CPU(MemoryControlUnit memoryControlUnit, ProgrammableInterruptController pic) : this(memoryControlUnit, pic, NullLogger<CPU>.Instance)
    {
    }

    public CPU(MemoryControlUnit memoryControlUnit, ProgrammableInterruptController pic, ILogger logger)
    {
        ALU = new();
        ControlUnit = new();
        MemoryControlUnit = memoryControlUnit;
        PIC = pic;

        this.logger = logger;
    }

    public void Boot()
    {
        ControlUnit.PC = DefaultPCAddress;
        ControlUnit.Privileged = true;

        for (int i = 0; i < 8; i++)
            ALU.RegisterFile[i] = 0;

        ControlUnit.ClockEnable = true;
    }

    public void Run(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && ControlUnit.ClockEnable)
        {
            Step();
        }
    }

    public void Step()
    {
        Fetch();
        var opcode = Decode();
        Execute(opcode);

        ControlUnit.MCC++; // increase Machine Cycle Counter every cycle

        // if timer interrupt is enabled and MCC is greater than TimerCycleInterval, fire timer interrupt
        if (ControlUnit.ClockEnable && ControlUnit.TimerInterruptEnable && ControlUnit.MCC > ControlUnit.TimerCycleInterval) 
        {
            // raise the timer flag

            ControlUnit.TimerInterruptEnable = true;
            ControlUnit.MCC = 0;
        }


        // process signal from PIC
        if (PIC.TryGetNextSignal(ControlUnit.Priority, out var signal))
        {
            CallInterrupt(signal!.interruptVector, signal!.priorityLevel);
        }

    }
    private void CallInterrupt(byte interruptVector, PriorityLevels priorityLevel)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Firing interrupt {i}...", interruptVector);
        }

        if (!ControlUnit.Privileged)
        {
            // backup user stack pointer
            SavedUSP = ALU.RegisterFile[6];
            // and restore system stack pointer
            ALU.RegisterFile[6] = (short)SavedSSP;

            ControlUnit.Privileged = true;
        }

        // push PSR and PC onto the stack
        ALU.RegisterFile[6] -= 1;
        MemoryControlUnit.MAR = (ushort)ALU.RegisterFile[6];
        MemoryControlUnit.MDR = (short)ControlUnit.PSR;
        MemoryControlUnit.WriteSignal(!ControlUnit.Privileged);
        ALU.RegisterFile[6] -= 1;
        MemoryControlUnit.MAR = (ushort)ALU.RegisterFile[6];
        MemoryControlUnit.MDR = (short)ControlUnit.PC;

        ControlUnit.Priority = (ushort)priorityLevel;

        // clear N, Z, P flags
        ControlUnit.PSR = (ushort)(ControlUnit.PSR & 0xFFF8);

        // read the interrupt vector
        MemoryControlUnit.MAR = (ushort)(interruptVector | 0x100);
        MemoryControlUnit.ReadSignal(!ControlUnit.Privileged);
        ControlUnit.PC = (ushort)MemoryControlUnit.MDR;
    }

    public Opcodes Decode() { 
        return (Opcodes)(ControlUnit.IR >> 12); 
    }

    public void Execute(Opcodes opcode)
    {
        switch (opcode)
        {
            case Opcodes.ADD:
                Add();
                break;
            case Opcodes.AND:
                And();
                break;
            case Opcodes.NOT:
                Not();
                break;
            case Opcodes.LD:
                Load();
                break;
            case Opcodes.LDI:
                LoadIndirect();
                break;
            case Opcodes.LDR:
                LoadRegister();
                break;
            case Opcodes.LEA:
                LoadEffectiveAddress();
                break;
            case Opcodes.ST:
                Store();
                break;
            case Opcodes.STI:
                StoreIndirect();
                break;
            case Opcodes.STR:
                StoreRegister();
                break;
            case Opcodes.BR:
                Branch();
                break;
            case Opcodes.JMP:
                Jump();
                break;
            case Opcodes.JSR:
                JumpToSubroutine();
                break;
            case Opcodes.RTI:
                ReturnFromInterrupt();
                break;
            case Opcodes.TRAP:
                Trap();
                break;
            case Opcodes.Reserved:
                break;
            default:
                throw new InvalidOperationException("Invalid opcode");
        }
    }

    public void Fetch()
    {
        MemoryControlUnit.MAR = ControlUnit.PC++;
        MemoryControlUnit.ReadSignal(!ControlUnit.Privileged);
        ControlUnit.IR = (ushort)MemoryControlUnit.MDR;
    }

    #region Opcodes
    private void Add()
    {
        var dr = (ControlUnit.IR >> 9) & 0x7;
        var sr1 = (ControlUnit.IR >> 6) & 0x7;
        var immFlag = (ControlUnit.IR >> 5) & 0x1;
        if (immFlag == 1)
        {
            ushort imm5 = (ushort)(ControlUnit.IR & 0x1F);
            if ((imm5 & 0x10) == 0x10)
            {
                imm5 |= 0xFFE0; // sign extend
            }
            ALU.RegisterFile[dr] = (short)(ALU.RegisterFile[sr1] + (short)imm5);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("ADD R{dr} = R{sr1} + x{value:X} = x{r:X}", dr, sr1, (short)imm5, ALU.RegisterFile[dr]);
            }
        }
        else
        {
            var sr2 = ControlUnit.IR & 0x7;
            ALU.RegisterFile[dr] = (short)(ALU.RegisterFile[sr1] + ALU.RegisterFile[sr2]);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("ADD R{dr} = R{sr1} + R{sr2} = x{r:X}", dr, sr1, sr2, ALU.RegisterFile[dr]);
            }
        }

        CalculateNZP(ALU.RegisterFile[dr]);
    }

    private void And()
    {
        var dr = (ControlUnit.IR >> 9) & 0x7;
        var sr1 = (ControlUnit.IR >> 6) & 0x7;
        var immFlag = (ControlUnit.IR >> 5) & 0x1;
        if (immFlag == 1)
        {
            ushort imm5 = (ushort)(ControlUnit.IR & 0x1F);
            if ((imm5 & 0x10) == 0x10)
            {
                imm5 |= 0xFFE0; // sign extend
            }
            ALU.RegisterFile[dr] = (short)(ALU.RegisterFile[sr1] & imm5);
            CalculateNZP(ALU.RegisterFile[dr]);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("AND R{dr} = R{sr1} & x{value:X} = x{r:X}", dr, sr1, (short)imm5, ALU.RegisterFile[dr]);
            }
        }
        else
        {
            var sr2 = ControlUnit.IR & 0x7;
            ALU.RegisterFile[dr] = (short)(ALU.RegisterFile[sr1] & ALU.RegisterFile[sr2]);

            CalculateNZP(ALU.RegisterFile[dr]);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("AND R{dr} = R{sr1} & R{sr2} = x{r:X}", dr, sr1, sr2, ALU.RegisterFile[dr]);
            }
        }
    }

    private void Not()
    {
        var dr = (ControlUnit.IR >> 9) & 0x7;
        var sr = (ControlUnit.IR >> 6) & 0x7;
        ALU.RegisterFile[dr] = (short)(~ALU.RegisterFile[sr]);

        CalculateNZP(ALU.RegisterFile[dr]);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("NOT R{dr} = !x{value:X}", dr, ALU.RegisterFile[dr]);
        }
    }

    private void Load()
    {
        var dr = (ControlUnit.IR >> 9) & 0x7;
        MemoryControlUnit.MAR = EvaluatePCRelativeAddress9();
        MemoryControlUnit.ReadSignal(!ControlUnit.Privileged);
        ALU.RegisterFile[dr] = MemoryControlUnit.MDR;

        CalculateNZP(MemoryControlUnit.MDR);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("LD R{dr} with x{value:X} from [x{addr:X}]", dr, ALU.RegisterFile[dr], MemoryControlUnit.MAR);
        }
    }

    private void LoadIndirect()
    {
        // LDI
        var dr = (ControlUnit.IR >> 9) & 0x7;
        MemoryControlUnit.MAR = EvaluatePCRelativeAddress9();
        MemoryControlUnit.ReadSignal(!ControlUnit.Privileged);
        MemoryControlUnit.MAR = (ushort)MemoryControlUnit.MDR;
        MemoryControlUnit.ReadSignal(!ControlUnit.Privileged);
        ALU.RegisterFile[dr] = MemoryControlUnit.MDR;

        CalculateNZP(MemoryControlUnit.MDR);

        if (logger.IsEnabled(LogLevel.Debug)) {
            logger.LogDebug("LDI R{dr} with x{value:X} from [x{addr:X}]", dr, ALU.RegisterFile[dr], MemoryControlUnit.MAR);
        }
    }

    private void LoadRegister()
    {
        // LDR
        var dr = (ControlUnit.IR >> 9) & 0x7;
        var baseR = (ControlUnit.IR >> 6) & 0x7;

        ushort offset6 = (ushort)(ControlUnit.IR & 0x3F);
        if ((ControlUnit.IR & 0x20) == 0x20)
        {
            offset6 |= 0xFFC0;
        }
        MemoryControlUnit.MAR = (ushort)(ALU.RegisterFile[baseR] + offset6);
        MemoryControlUnit.ReadSignal(!ControlUnit.Privileged);
        ALU.RegisterFile[dr] = MemoryControlUnit.MDR;

        CalculateNZP(MemoryControlUnit.MDR);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("LDR R{dr} = x{value:X} from [x{addr:X}]", dr, ALU.RegisterFile[dr], MemoryControlUnit.MAR);
        }
    }

    private void LoadEffectiveAddress()
    {
        // LEA
        var dr = (ControlUnit.IR >> 9) & 0x7;
        ALU.RegisterFile[dr] = (short)EvaluatePCRelativeAddress9();
        
        CalculateNZP(MemoryControlUnit.MDR);
        
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("LEA [x{dr:X}](x{v:X}) into R{dr}", ALU.RegisterFile[dr], MemoryControlUnit.MDR, dr);
        }
    }

    private void Store()
    {
        // ST
        var sr = (ControlUnit.IR >> 9) & 0x7;
        MemoryControlUnit.MAR = EvaluatePCRelativeAddress9();
        MemoryControlUnit.MDR = ALU.RegisterFile[sr];
        MemoryControlUnit.WriteSignal(!ControlUnit.Privileged); 

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("ST R{sr} with x{value:X} to [x{addr:X}]", sr, ALU.RegisterFile[sr], MemoryControlUnit.MAR);
        }
    }

    private void StoreIndirect()
    {
        // STI
        var sr = (ControlUnit.IR >> 9) & 0x7;
        MemoryControlUnit.MAR = EvaluatePCRelativeAddress9();
        MemoryControlUnit.ReadSignal(!ControlUnit.Privileged);
        MemoryControlUnit.MAR = (ushort)MemoryControlUnit.MDR;
        MemoryControlUnit.MDR = ALU.RegisterFile[sr];
        MemoryControlUnit.WriteSignal(!ControlUnit.Privileged);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("STI R{sr} with x{value:X} to [x{addr:X}]", sr, ALU.RegisterFile[sr], MemoryControlUnit.MAR);
        }
    }

    private void StoreRegister()
    {
        // STR
        var sr = (ControlUnit.IR >> 9) & 0x7;
        var baseR = (ControlUnit.IR >> 6) & 0x7;
        ushort offset6 = (ushort)(ControlUnit.IR & 0x3F);
        if ((ControlUnit.IR & 0x20) == 0x20)
        {
            offset6 |= 0xFFC0;
        }
        MemoryControlUnit.MAR = (ushort)(ALU.RegisterFile[baseR] + offset6);
        MemoryControlUnit.MDR = ALU.RegisterFile[sr];
        MemoryControlUnit.WriteSignal(!ControlUnit.Privileged);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("STR R{sr} with x{value:X} to [x{addr:X}]", sr, ALU.RegisterFile[sr], MemoryControlUnit.MAR);
        }
    }

    private void Branch()
    {
        if ((ControlUnit.N && ((ControlUnit.IR >> 11) & 0x1) == 1) 
            || (ControlUnit.Z && ((ControlUnit.IR >> 10) & 0x1) == 1)
            || (ControlUnit.P && ((ControlUnit.IR >> 9) & 0x1) == 1))
        {
            ControlUnit.PC = EvaluatePCRelativeAddress9();

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("BR{n}{z}{p} to x{addr:X}",
                    (((ControlUnit.IR >> 11) & 0x1) == 1) ? "n" : string.Empty,
                    (((ControlUnit.IR >> 10) & 0x1) == 1) ? "z" : string.Empty,
                    (((ControlUnit.IR >> 9) & 0x1) == 1) ? "p" : string.Empty,
                    ControlUnit.PC);
            }
        }
        else if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("BR skipped");
        }
    }

    private void Jump()
    {
        var baseR = (ControlUnit.IR >> 6) & 0x7;
        ControlUnit.PC = (ushort)ALU.RegisterFile[baseR];

        if ((ControlUnit.IR & 0x1) == 0x1) // bit 0 is set means it's a JMPT instruction (new, not in the book)
        {
            ControlUnit.Privileged = false;
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("JMP to R{baseR} with x{addr:X}", baseR, ControlUnit.PC);
        }
    }

    private void Trap()
    {
        var trapVector = (ushort)(ControlUnit.IR & 0xFF);
        ALU.RegisterFile[7] = (short)ControlUnit.PC;
        MemoryControlUnit.MAR = trapVector;
        MemoryControlUnit.ReadSignal(false);
        ControlUnit.PC = (ushort)MemoryControlUnit.MDR;

        if (!ControlUnit.Privileged)
        {
            SavedUSP = ALU.RegisterFile[6];
            ALU.RegisterFile[6] = (short)SavedSSP;
            ControlUnit.Privileged = true;
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("TRAP x{v:X} to x{addr:X}", trapVector, ControlUnit.PC);
        }
    }

    private void JumpToSubroutine()
    {
        // JSR
        var longFlag = (ControlUnit.IR >> 11) & 0x1;
        var baseR = (ControlUnit.IR >> 6) & 0x7;
        if (longFlag == 1)
        {
            ushort pcOffset11 = (ushort)(ControlUnit.IR & 0x7FF);
            ALU.RegisterFile[7] = (short)ControlUnit.PC;
            ControlUnit.PC += pcOffset11;

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("JSR (short) to x{addr:X}", ControlUnit.PC);
            }
        }
        else
        {
            ControlUnit.PC = (ushort)ALU.RegisterFile[baseR];

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("JSRR (long) to x{addr:X}", ControlUnit.PC);
            }
        }
    }

    private void ReturnFromInterrupt()
    {
        if (!ControlUnit.Privileged)
        {
            throw new PrivilegeModeException();
        }

        if (!ControlUnit.Privileged)
        {
            SavedSSP = ALU.RegisterFile[6];
            ALU.RegisterFile[6] = (short)SavedUSP;
        }

        // pop PSR and PC from the stack
        ushort r6 = (ushort)ALU.RegisterFile[6];

        MemoryControlUnit.MAR = r6++;
        MemoryControlUnit.ReadSignal(!ControlUnit.Privileged);
        ControlUnit.PC = (ushort)MemoryControlUnit.MDR;

        MemoryControlUnit.MAR = r6++;
        MemoryControlUnit.ReadSignal(!ControlUnit.Privileged);
        ControlUnit.PSR = (ushort)MemoryControlUnit.MDR;

        ALU.RegisterFile[6] = (short)r6;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("RTI to x{addr:X}", ControlUnit.PC);
        }
    }

    private void CalculateNZP(short v)
    {
        ControlUnit.N = v < 0;
        ControlUnit.Z = v == 0;
        ControlUnit.P = v > 0;
    }

    private ushort EvaluatePCRelativeAddress6()
    {
        ushort pcOffset6 = (ushort)(ControlUnit.IR & 0x3F);
        if ((ControlUnit.IR & 0x20) == 0x20)
        {
            pcOffset6 |= 0xFFC0;
        }
        return (ushort)(ControlUnit.PC + (short)pcOffset6);
    }

    private ushort EvaluatePCRelativeAddress9()
    {
        ushort pcOffset9 = (ushort)(ControlUnit.IR & 0x1FF);
        if ((ControlUnit.IR & 0x100) == 0x100)
        {
            pcOffset9 |= 0xFE00;
        }
        return (ushort)(ControlUnit.PC + (short)pcOffset9);
    }

    public void OnReadSignal(ushort address, out short value)
    {
        if (address == TMR_ADDRESS)
        {
            value = (short)(ControlUnit.TimerInterruptEnable ? 0x8000 : 0x0000);
        }
        else if (address == TMI_ADDRESS)
        {
            value = (short)ControlUnit.TimerCycleInterval;
        }
        else if (address == MPR_ADDRESS)
        {
            value = (short)MemoryControlUnit.MPR;
        }
        else if (address == MCR_ADDRESS)
        {
            value = (short)ControlUnit.MCR;
        }
        else if (address == MCC_ADDRESS)
        {
            value = (short)ControlUnit.MCC;
        }
        else
        {
            value = 0;
        }
    }

    public void OnWriteSignal(ushort address, short value)
    {
        if (address == TMR_ADDRESS)
        {
            ControlUnit.TimerInterruptEnable = ((ushort)value & 0x8000) == 0x8000;
        }
        else if (address == TMI_ADDRESS)
        {
            ControlUnit.TimerCycleInterval = (ushort)value;
        }
        else if (address == MPR_ADDRESS)
        {
            MemoryControlUnit.MPR = (ushort)value;
        }
        else if (address == MCR_ADDRESS)
        {
            ControlUnit.MCR = (ushort)value;
        }
        else if (address == MCC_ADDRESS)
        {
            ControlUnit.MCC = (ushort)value;
        }
        else
        {
            value = 0;
        }
    }

    public void Attach(ILC3Machine machine)
    {
        machine.MemoryControlUnit.Map(MemoryRange.FromStartAndEnd(TMR_ADDRESS, MCC_ADDRESS), this);

        IsAttached = true;
    }

    public void Detach()
    {
    }
    #endregion
}
