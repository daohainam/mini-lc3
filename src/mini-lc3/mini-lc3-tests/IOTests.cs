

using mini_lc3_vm.Devices;
using System.Text;

namespace mini_lc3_tests;

public class IOTests
{
    private LC3Machine _machine;

    public IOTests()
    {
        _machine = new LC3Machine();
        _machine.CPU.Boot();
    }

    [Theory]
    [InlineData("Hello, World!\n")]
    [InlineData("We know that the Calculator.Add() function is working correctly for these specific values, but we'll clearly need to test more values than just 1 and 2. The question is, what's the best way to achieve this? We could copy and paste the test and just change the specific values used for each one, but that's a bit messy. Instead, xUnit provides the [Theory] attribute for this situation.\n")]
    [InlineData("This is a test of the LC3 Keyboard device.\n")]
    [InlineData("LC-3 has 1 input and 1 out device. Both devices are built-in, so no additional drivers are needed to access either one\n")]
    public void Keyboard_Read_Test(string inputString)
    {
        var outputString = new StringBuilder();
        var console = new MemoryConsoleDevice();
        _machine.AttachDevice(new LC3Keyboard(console));

        console.SimulateKeyboardTyping(inputString);

        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        cancellationTokenSource.CancelAfter(inputString.Length * 100);

        var state = _machine.CPU.MemoryControlUnit.Read(LC3Keyboard.KBSR_ADDRESS);
        do
        {
            if ((state & 0x8000) == 0x8000) // is bit[15] on?
            {
                var character = _machine.CPU.MemoryControlUnit.Read(LC3Keyboard.KBDR_ADDRESS);

                var ascii = (char)character;
                Console.Write(ascii);
                outputString.Append(ascii);

                if (character == 0x0A) // is it a newline?
                {
                    break;
                }
            }

            state = _machine.CPU.MemoryControlUnit.Read(LC3Keyboard.KBSR_ADDRESS);
        } while (!cancellationToken.IsCancellationRequested);

        outputString.ToString().Should().Be(inputString);
    }
}
