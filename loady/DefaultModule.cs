using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loady
{
    public class DefaultModule : Module
    {
        public Agent Create(int index, Agent.Config config)
        {
            return new loady.Agent(index, config);
        }
    }
}
