﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Components;

public class ControlUnit
{
    public ushort IR { get; set; }
    public ushort PC { get; set; }
    public ushort PSR { get; set; }
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
        get => (PSR & 0x80) == 0x4;
        set => PSR = (ushort)(value ? PSR | 0x80 : PSR & ~0x80);
    }
    public ushort Priority
    {
        get => (ushort)((PSR & 0x70) >> 8);
        set => PSR = (ushort)((PSR & ~0x70) | ((value & 0x07) << 8));
    }
}
