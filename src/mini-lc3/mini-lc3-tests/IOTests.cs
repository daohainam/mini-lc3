

using mini_lc3_vm.Devices;

namespace mini_lc3_tests;

public class IOTests
{
    private LC3Machine _machine;

    public IOTests()
    {
        _machine = new LC3Machine();
        _machine.CPU.Boot();
    }

    [Fact]
    public void Keyboard_Reads()
    {
        var console = new MemoryConsoleDevice();
        _machine.AttachDevice(new LC3Keyboard(console));

        console.SimulateKeyboardTyping("Hello, World!\n");

        var state = _machine.CPU.MemoryControlUnit.Read(LC3Keyboard.KBSR_ADDRESS);
        do
        {
            if ((state & 0x8000) == 0x8000) // is bit[15] on?
            {
                var character = _machine.CPU.MemoryControlUnit.Read(LC3Keyboard.KBDR_ADDRESS);

                var ascii = (char)character;
                Console.Write(ascii);

                if (character == 0x0A) // is it a newline?
                {
                    break;
                }
            }

            state = _machine.CPU.MemoryControlUnit.Read(LC3Keyboard.KBSR_ADDRESS);
        } while (true);
    }
}
