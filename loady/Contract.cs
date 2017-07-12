using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loady
{
    public class Contract
    {
        public static void Assert(Boolean v)
        {
            System.Diagnostics.Contracts.Contract.Assert(v);
        }

        public static void Assert(Boolean v, string m)
        {
            System.Diagnostics.Contracts.Contract.Assert(v, m);
        }
    }
}
