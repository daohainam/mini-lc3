using mini_lc3_vm.ExecuteableFile;

namespace mini_lc3_vm.ProgramLoaders;
internal class ObjExecutableImageLoader(string path) : IExecutableImageLoader
{
    public IExecutableImage LoadExecutableFile()
    {
        byte[] fileBytes = File.ReadAllBytes(path);
        ushort[] instructions = new ushort[fileBytes.Length / 2];
        Buffer.BlockCopy(fileBytes, 0, instructions, 0, fileBytes.Length);

        return new ExecutableImage(instructions[0], instructions.Skip(1).ToArray());
    }
}
