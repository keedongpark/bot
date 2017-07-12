using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loady
{
    /// <summary>
    ///  
    /// </summary>
    public interface Module
    {
        Agent Create(int index, Agent.Config config);
    }
}
