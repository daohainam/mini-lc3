namespace mini_lc3_vm.ProgramLoaders;

public class LoaderFactory
{
    public static IExecutableImageLoader GetLoader(string filename)
    {
        if (filename.EndsWith(".bin", StringComparison.OrdinalIgnoreCase))
        {
            return new BinExecutableImageLoader(filename);
        }
        else if (filename.EndsWith(".hex", StringComparison.OrdinalIgnoreCase))
        {
            return new HexExecutableImageLoader(filename);
        }
        else
        {
            throw new ArgumentException("Unknown file type");
        }
    }
}
