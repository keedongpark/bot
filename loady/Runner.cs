using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace loady
{
    /// <summary>
    /// Runner that runs agents
    /// </summary>
    public class Runner
    {
        List<Agent> agents = new List<Agent>(); 
        volatile bool stop = false;
        int sleep = 1;
        Thread thread;

        public Runner(int sleep = 1)
        {
            this.sleep = sleep;

            Contract.Assert(this.sleep > 0);
        }

        public void Add(Agent agent)
        {
            Contract.Assert(agent != null);

            agents.Add(agent);
        }

        public bool Start()
        {
            Contract.Assert(agents.Count > 0);

            foreach ( var agent in agents )
            {
                agent.Start();
            }

            thread = new Thread(this.Run);
            thread.Start();

            return thread.IsAlive;
        }

        public void Run()
        {
            while ( !stop )
            {
                foreach ( var agent in agents )
                {
                    agent.Execute();
                }

                Thread.Sleep(sleep);
            }
        }

        public void Stop()
        {
            Contract.Assert(thread != null);
            Contract.Assert(!stop);

            stop = true;

            thread.Join();

            Contract.Assert(!thread.IsAlive);
        }
    }
}
