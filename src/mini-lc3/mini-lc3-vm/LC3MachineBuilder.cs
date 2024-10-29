namespace mini_lc3_vm;

public class LC3MachineBuilder: ILC3MachineBuilder
{
    public ILC3Machine Build()
    {
        return new LC3Machine();
    }
}
