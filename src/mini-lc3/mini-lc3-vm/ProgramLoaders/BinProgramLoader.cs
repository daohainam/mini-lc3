namespace mini_lc3_vm.ProgramLoaders;

using mini_lc3_vm;
internal class BinProgramLoader : IProgramLoader
{
    public string path;
    public BinProgramLoader(string path)
    {
        this.path = path;
    }

    public short[] LoadProgram()
    {
        var lines = File.ReadAllLines(path);
        var program = new short[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            program[i] = Convert.ToInt16(lines[i], 2);
        }

        return program;
    }
}
