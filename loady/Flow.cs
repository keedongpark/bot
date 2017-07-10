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

        string desc = ""; 
        int configRepeat = 0; // -1 : infinite, 0: once, else: count
        int repeated = 0;

        public int Index { get { return index; } }

        public List<Act> Acts { get { return acts; } }

        /// <summary>
        /// To copy
        /// </summary>
        private Flow()
        {

        }

        public Flow(YamlMappingNode def)
        {
            if ( def.Children.ContainsKey("desc"))
            {
                desc = ((YamlScalarNode)def.Children["desc"]).Value;
            }

            if ( def.Children.ContainsKey("repeat"))
            {
                configRepeat = Int32.Parse(((YamlScalarNode)def.Children["repeat"]).Value);
            }

            // acts에 대해 act들 로딩
            var actsNode = (YamlSequenceNode)def.Children["acts"];

            for ( int i=0; i<actsNode.Children.Count; ++i)
            {
                var act = new Act(i, (YamlMappingNode)actsNode.Children[i]);
                acts.Add(act);
            }
        }

        public void Set(Agent agent)
        {
            this.agent = agent;

            foreach ( var act in acts)
            {
                act.Set(agent);
            }
        }

        public Flow Clone(Agent agent)
        {
            var nflow = new Flow();

            nflow.agent = agent;

            foreach ( var act in acts)
            {
                acts.Add(act.Clone(agent));
            }

            return nflow;
        }

        public void Start()
        {
            index = 0;
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
            nflow
        }
    }
}
