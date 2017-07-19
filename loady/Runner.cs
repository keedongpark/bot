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

        /// <summary>
        /// Agent를 추가. 단, Start 호출 전에만 가능.
        /// </summary>
        /// <param name="agent"></param>
        public void Add(Agent agent)
        {
            Contract.Assert(agent != null);
            Contract.Assert(thread == null || thread.IsAlive == false);

            agents.Add(agent);
        }

        /// <summary>
        /// Start all agents and starts a thread to execute agents in the thread.
        /// </summary>
        /// <returns>true if thread is alive</returns>
        public bool Start()
        {
            Contract.Assert(thread == null || thread.IsAlive == false);
            Contract.Assert(stop == false);

            foreach ( var agent in agents )
            {
                agent.Start();
            }

            thread = new Thread(this.Run);
            thread.Start();

            return thread.IsAlive;
        }

        /// <summary>
        /// 실행 함수. 
        /// </summary>
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

        /// <summary>
        /// 모든 에이전트가 완료 했는 지 확인
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 실행 쓰레드를 종료
        /// </summary>
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
