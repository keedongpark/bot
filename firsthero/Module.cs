using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loady;

namespace firsthero
{
    public class Module : loady.Module
    {
        public Agent Create(int index, Agent.Config config)
        {
            return new HeroAgent(index, config);
        }
    }
}
