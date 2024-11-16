using mini_lc3_vm.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Devices
{
    public class LC3GraphicalDisplay : IMappedMemory, IAttachable
    {
        public const ushort VRAM_ADDRESS_START = 0xC000;
        public const ushort VRAM_ADDRESS_END = 0xFDFF;
        private readonly short [] videoRam = new short[VRAM_ADDRESS_END - 0xC000 + 1]; // LC3 supports 128x124 graphical display, so it needs 15872 word in video ram

        private readonly IGraphicalDisplayDevice graphicalDisplayDevice;

        public LC3GraphicalDisplay(IGraphicalDisplayDevice graphicalDisplayDevice)
        {
            this.graphicalDisplayDevice = graphicalDisplayDevice;
        }

        public void OnReadSignal(ushort address, out short value)
        {
            if (address >= VRAM_ADDRESS_START && address <= VRAM_ADDRESS_END)
            {
                value = (short)(videoRam[address - VRAM_ADDRESS_START]);
            }
            else
            {
                value = 0;
            }
        }

        public void OnWriteSignal(ushort address, short value)
        {
            if (address >= VRAM_ADDRESS_START && address <= VRAM_ADDRESS_END)
            {
                ushort addr = (ushort)(address - VRAM_ADDRESS_START);

                videoRam[address - VRAM_ADDRESS_START] = value;
                graphicalDisplayDevice.Render(new Span<short>(videoRam), addr);
            }
        }

        public void Attach(ILC3Machine machine)
        {
            machine.MemoryControlUnit.Map(MemoryRange.FromStartAndEnd(VRAM_ADDRESS_START, VRAM_ADDRESS_END), this);

            IsAttached = true;
        }

        public void Detach()
        {
            IsAttached = false;
        }
        public bool IsAttached { get; set; } = false;
    }
}
