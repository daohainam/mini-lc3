namespace mini_lc3_vm.ProgramLoaders;

using mini_lc3_vm;
internal class HexProgramLoader : IProgramLoader
{
    public string path;
    public HexProgramLoader(string path)
    {
        this.path = path;
    }

    public ushort[] LoadProgram()
    {
        var lines = File.ReadAllLines(path);
        var program = new ushort[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            program[i] = Convert.ToUInt16(lines[i], 16);
        }

        return program;
    }
}
