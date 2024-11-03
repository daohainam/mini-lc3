namespace mini_lc3_vm.ProgramLoaders;
internal class BinExecutableImageLoader(string path) : InstructionPerLineExecutableImageLoader(path, 2)
{
}
