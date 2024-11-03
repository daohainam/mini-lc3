namespace mini_lc3_vm.ProgramLoaders;
internal class HexExecutableImageLoader(string path) : InstructionPerLineExecutableImageLoader(path, 16)
{
}
