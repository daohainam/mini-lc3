
namespace mini_lc3_vm
{
    public interface ILC3Machine
    {
        void Run(CancellationToken cancellationToken);
    }
}
