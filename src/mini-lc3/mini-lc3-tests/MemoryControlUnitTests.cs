using mini_lc3_vm.Components;

namespace mini_lc3_tests;

public class MemoryControlUnitTests
{
    private Memory _memory;
    private MemoryControlUnit _mcu;

    public MemoryControlUnitTests()
    {
        _memory = new Memory();
        _memory.Reset();
        _mcu = new MemoryControlUnit(_memory);
    }

    [Fact]
    public void ReadSignal_UnmappedDeviceAddress_ReturnsZero()
    {
        // Arrange: Map a device to a specific range in the I/O page
        var device = new StubMappedMemory(readValue: 0x42);
        _mcu.Map(MemoryRange.FromStartAndEnd(0xFE00, 0xFE01), device);

        // Act: Read from 0xFE04 which is >= lowestDeviceMappedAddress (0xFE00)
        // but is NOT in any mapped device range.
        // Before the fix, this would fall through to reading raw memory
        // (causing IndexOutOfRangeException for I/O page addresses).
        _mcu.MAR = 0xFE04;
        _mcu.ReadSignal(false);

        // Assert: Should return 0 for unmapped device address
        _mcu.MDR.Should().Be(0);
    }

    [Fact]
    public void ReadSignal_MappedDeviceAddress_ReturnsDeviceValue()
    {
        // Arrange: Map a device that returns a specific value
        var device = new StubMappedMemory(readValue: 0x42);
        _mcu.Map(MemoryRange.FromStartAndEnd(0xFE00, 0xFE01), device);

        // Act
        _mcu.MAR = 0xFE00;
        _mcu.ReadSignal(false);

        // Assert
        _mcu.MDR.Should().Be(0x42);
    }

    [Fact]
    public void ReadSignal_NormalMemoryAddress_ReturnsMemoryValue()
    {
        // Arrange: Map a device to a high address
        var device = new StubMappedMemory();
        _mcu.Map(MemoryRange.FromStartAndEnd(0xFE00, 0xFE01), device);

        // Write a value to normal memory below the device range
        _memory[0x3000] = 0x5678;

        // Act: Read from normal memory
        _mcu.MAR = 0x3000;
        _mcu.ReadSignal(false);

        // Assert: Should return the memory value
        _mcu.MDR.Should().Be(0x5678);
    }

    /// <summary>
    /// Simple stub device for testing memory-mapped I/O.
    /// </summary>
    private class StubMappedMemory : IMappedMemory
    {
        private readonly short _readValue;

        public StubMappedMemory(short readValue = 0)
        {
            _readValue = readValue;
        }

        public void OnReadSignal(ushort address, out short value)
        {
            value = _readValue;
        }

        public void OnWriteSignal(ushort address, short value)
        {
        }
    }
}
