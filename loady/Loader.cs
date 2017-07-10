using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loady
{
    public class Loader
    {
        List<Agent> agents = new List<Agent>();
        List<Runner> runers = new List<Runner>();

        public List<Agent> Agents { get { return agents; } }

        public bool Load(string filename)
        {
            // 

            return true;
        }

        public void Start()
        {
            // starts all agents
        }

        public void Stop()
        {
            // stop all agents
        }
    }
}
