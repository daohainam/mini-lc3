namespace mini_lc3_vm.ProgramLoaders;

using mini_lc3_vm.ExecuteableFile;

internal abstract class InstructionPerLineExecutableImageLoader : IExecutableImageLoader
{
    public readonly string path;
    private readonly int fromBase;

    public InstructionPerLineExecutableImageLoader(string path, int fromBase)
    {
        this.path = path;
        this.fromBase = fromBase;
    }

    public IExecutableImage LoadExecutableFile()
    {
        var lines = File.ReadAllLines(path);
        if (lines.Length == 0)
            throw new InvalidExecutableImageException("Empty file");

        var image = new ExecutableImage(Convert.ToUInt16(lines[0], fromBase), lines.Length - 1);
        for (int i = 1; i < lines.Length; i++)
        {
            image.Instructions[i - 1] = Convert.ToUInt16(lines[i], fromBase);
        }

        return image;
    }
}
