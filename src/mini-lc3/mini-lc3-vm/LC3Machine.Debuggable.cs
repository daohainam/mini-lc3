using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mini_lc3_vm
{
    public partial class LC3Machine
    {
        private readonly IDictionary<ushort, BreakPoint> breakPoints = new Dictionary<ushort, BreakPoint>();
        
        public void AddBreakPoint(BreakPoint breakPoint)
        {
            breakPoints[breakPoint.Address] = breakPoint;
        }
        public bool RemoveBreakPoint(ushort address)
        {
            return breakPoints.Remove(address);
        }
    }
}
