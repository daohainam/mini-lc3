using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm.Components;

public interface IAttachable
{
    void Attach(ILC3Machine lC3Machine);
    void Detach();
    bool IsAttached { get; }
}
