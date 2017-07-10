using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using loady;

namespace loady
{
    public class Flow
    {
        Agent agent;
        List<Act> acts = new List<Act>();
        int index = -1;

        int configRepeat = 0; // -1 : infinite, 0: once, else: count
        int repeated = 0;

        public int Index { get { return index; } }

        public List<Act> Acts { get { return acts; } }

        public Flow(Agent agent, int index, YamlMappingNode def)
        {
            this.index = index;
            this.agent = agent;

            // flow 구성 로딩 

            // acts에 대해 act들 로딩
        }

        public void Do()
        {
            acts[index].Do();
        }

        public void On(Msg m)
        {
            acts[index].On(m);
        }

        public void Next()
        {
            index++;

            if ( index >= acts.Count)
            {
                if (configRepeat < 0 || (configRepeat > 0 && repeated < configRepeat))
                {
                    repeated++;
                    index = 0;
                }
                else
                {
                    agent.Complete(false, "end of act reached");
                }
            }
        }

        public void Jump(int index)
        {
            if ( index >= 0 && index < acts.Count)
            {
                this.index = index;
            }
        }
    }
}
