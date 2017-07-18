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

        /// <summary>
        /// Get Agents count
        /// </summary>
        public int Count {  get { return agents.Count; } }

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

        private void Run()
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

        public bool IsCompleted()
        {
            foreach ( var agent in agents )
            {
                if ( !agent.IsCompleted )
                {
                    return false;
                }
            }

            return true;
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
