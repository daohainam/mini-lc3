namespace mini_lc3_vm.Components;

public class CPU
{
    public const ushort DefaultPCAddress = 0x3000;

    public ArithmeticLogicUnit ALU { get; } 
    public ControlUnit ControlUnit { get; }
    public MemoryControlUnit MemoryControlUnit { get; }

    public CPU(MemoryControlUnit memoryControlUnit)
    {
        ALU = new();
        ControlUnit = new();
        MemoryControlUnit = memoryControlUnit;
    }

    public void Boot()
    {
        ControlUnit.PC = DefaultPCAddress;
        for (int i = 0; i < 8; i++)
            ALU.RegisterFile[i] = 0;
    }

    public void Run(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Fetch();
            var opcode = Decode();
            Execute(opcode);
        }
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
        MemoryControlUnit.ReadSignal();
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
            var imm5 = ControlUnit.IR & 0x1F;
            if ((imm5 & 0x10) == 0x10)
            {
                imm5 |= 0xFFE0; // sign extend
            }
            ALU.RegisterFile[dr] = (short)(ALU.RegisterFile[sr1] + imm5);
        }
        else
        {
            var sr2 = ControlUnit.IR & 0x7;
            ALU.RegisterFile[dr] = (short)(ALU.RegisterFile[sr1] + ALU.RegisterFile[sr2]);
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
            var imm5 = ControlUnit.IR & 0x1F;
            if ((imm5 & 0x10) == 0x10)
            {
                imm5 |= 0xFFE0; // sign extend
            }
            ALU.RegisterFile[dr] = (short)(ALU.RegisterFile[sr1] & imm5);
        }
        else
        {
            var sr2 = ControlUnit.IR & 0x7;
            ALU.RegisterFile[dr] = (short)(ALU.RegisterFile[sr1] & ALU.RegisterFile[sr2]);
        }

        CalculateNZP(ALU.RegisterFile[dr]);
    }

    private void Not()
    {
        var dr = (ControlUnit.IR >> 9) & 0x7;
        var sr = (ControlUnit.IR >> 6) & 0x7;
        ALU.RegisterFile[dr] = (short)(~ALU.RegisterFile[sr]);

        CalculateNZP(ALU.RegisterFile[dr]);
    }

    private void Load()
    {
        var dr = (ControlUnit.IR >> 9) & 0x7;
        MemoryControlUnit.MAR = EvaluatePCRelativeAddress9();
        MemoryControlUnit.ReadSignal();
        ALU.RegisterFile[dr] = MemoryControlUnit.MDR;

        CalculateNZP(MemoryControlUnit.MDR);
    }

    private void LoadIndirect()
    {
        var dr = (ControlUnit.IR >> 9) & 0x7;
        var pcOffset9 = ControlUnit.IR & 0x1FF;
        MemoryControlUnit.MAR = (ushort)(ControlUnit.PC + pcOffset9);
        MemoryControlUnit.ReadSignal();
        ALU.RegisterFile[dr] = MemoryControlUnit.MDR;

        CalculateNZP(MemoryControlUnit.MDR);
    }

    private void LoadRegister()
    {
        var dr = (ControlUnit.IR >> 9) & 0x7;
        var baseR = (ControlUnit.IR >> 6) & 0x7;
        var offset6 = ControlUnit.IR & 0x3F;
        MemoryControlUnit.MAR = (ushort)(ALU.RegisterFile[baseR] + offset6);
        MemoryControlUnit.ReadSignal();
        ALU.RegisterFile[dr] = MemoryControlUnit.MDR;

        CalculateNZP(MemoryControlUnit.MDR);
    }

    private void LoadEffectiveAddress()
    {
        var dr = (ControlUnit.IR >> 9) & 0x7;
        var pcOffset9 = ControlUnit.IR & 0x1FF;
        ALU.RegisterFile[dr] = (short)(ControlUnit.PC + pcOffset9);
    }

    private void Store()
    {
        var sr = (ControlUnit.IR >> 9) & 0x7;
        MemoryControlUnit.MAR = EvaluatePCRelativeAddress9();
        MemoryControlUnit.MDR = ALU.RegisterFile[sr];
        MemoryControlUnit.WriteSignal();
    }

    private void StoreIndirect()
    {
        var sr = (ControlUnit.IR >> 9) & 0x7;
        MemoryControlUnit.MAR = EvaluatePCRelativeAddress9();
        MemoryControlUnit.ReadSignal();
        MemoryControlUnit.MDR = ALU.RegisterFile[sr];
        MemoryControlUnit.WriteSignal();
    }

    private void StoreRegister()
    {
        var sr = (ControlUnit.IR >> 9) & 0x7;
        var baseR = (ControlUnit.IR >> 6) & 0x7;
        var offset6 = ControlUnit.IR & 0x3F;
        MemoryControlUnit.MAR = (ushort)(ALU.RegisterFile[baseR] + offset6);
        MemoryControlUnit.MDR = ALU.RegisterFile[sr];
        MemoryControlUnit.WriteSignal();
    }

    private void Branch()
    {
        if ((ControlUnit.N && ((ControlUnit.IR >> 11) & 0x1) == 1) 
            || (ControlUnit.Z && ((ControlUnit.IR >> 10) & 0x1) == 1)
            || (ControlUnit.P && ((ControlUnit.IR >> 9) & 0x1) == 1))
        {
            ControlUnit.PC = EvaluatePCRelativeAddress9();
        }
    }

    private void Jump()
    {
        var baseR = (ControlUnit.IR >> 6) & 0x7;
        ControlUnit.PC = (ushort)ALU.RegisterFile[baseR];
    }

    private void Trap()
    {
        var trapVector = (ushort)(ControlUnit.IR & 0xFF);
        ALU.RegisterFile[7] = (short)ControlUnit.PC;
        MemoryControlUnit.MAR = trapVector;
        MemoryControlUnit.ReadSignal();
        ControlUnit.PC = (ushort)MemoryControlUnit.MDR;
    }

    private void JumpToSubroutine()
    {
        var longFlag = (ControlUnit.IR >> 11) & 0x1;
        var baseR = (ControlUnit.IR >> 6) & 0x7;
        if (longFlag == 1)
        {
            ushort pcOffset11 = (ushort)(ControlUnit.IR & 0x7FF);
            ALU.RegisterFile[7] = (short)ControlUnit.PC;
            ControlUnit.PC += pcOffset11;
        }
        else
        {
            ControlUnit.PC = (ushort)ALU.RegisterFile[baseR];
        }
    }

    private void ReturnFromInterrupt()
    {
        ControlUnit.PC = (ushort)ALU.RegisterFile[7];
    }
    private void CalculateNZP(short v)
    {
        ControlUnit.N = v < 0;
        ControlUnit.Z = v == 0;
        ControlUnit.P = v > 0;
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
    #endregion
}
