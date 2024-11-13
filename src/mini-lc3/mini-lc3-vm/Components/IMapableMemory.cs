using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Components;

public interface IMapableMemory
{
    void Map(MemoryRange range, IMappedMemory device);
}

public class MemoryRange
{
    public ushort Start { get; }
    public ushort End { get; }

    private MemoryRange(ushort start, ushort end)
    {
        Start = start;
        End = end;
    }
    public bool Contains(int address)
    {
        return address >= Start && address <= End;
    }
    public int Length => End - Start + 1;

    public static MemoryRange FromStartAndEnd(ushort start, ushort end)
    {
        return new MemoryRange(start, end);
    }
    public bool CollidesWith(MemoryRange other)
    {
        return Contains(other.Start) || Contains(other.End) || other.Contains(Start) || other.Contains(End);
    }
}
