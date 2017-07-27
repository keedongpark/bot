using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using NLog;

namespace loady
{
    public class Flow
    {
        Agent agent;
        List<Act> acts = new List<Act>();
        List<Act> globalActs = new List<Act>();     // Flow 상에서 항상 실행되는 것들. 자체적으로 종료는 가능. 
        int index = -1;

        string desc = ""; 
        int configRepeat = 0; // -1 : infinite, 0: once, else: count
        int repeated = 0;

        Logger logger = LogManager.GetCurrentClassLogger();

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
            if (def.Children.ContainsKey("desc"))
            {
                desc = ((YamlScalarNode)def.Children["desc"]).Value;
            }

            if (def.Children.ContainsKey("repeat"))
            {
                configRepeat = Int32.Parse(((YamlScalarNode)def.Children["repeat"]).Value);
            }

            var actsNode = (YamlSequenceNode)def.Children["acts"];

            for (int i = 0; i < actsNode.Children.Count; ++i)
            {
                var act = new Act(i, (YamlMappingNode)actsNode.Children[i]);
                acts.Add(act);
            }

            if (def.Children.ContainsKey("globals"))
            {
                var alwaysNode = (YamlSequenceNode)def.Children["globals"];

                for (int i = 0; i < actsNode.Children.Count; ++i)
                {
                    var act = new Act(i, (YamlMappingNode)alwaysNode.Children[i]);
                    globalActs.Add(act);
                }
            }
        } 

        /// <summary>
        /// Bind a flow to an Agent
        /// Must be called before executing Acts
        /// </summary>
        /// <param name="agent"></param>
        public void Bind(Agent agent)
        {
            this.agent = agent;

            foreach ( var act in acts)
            {
                act.Set(agent);
            }
        }

        /// <summary>
        /// Just clone acts
        /// </summary>
        /// <returns></returns>
        public Flow Clone()
        {
            Contract.Assert(this.acts.Count > 0); // acts가 없는 플로우는 의미 없다.

            var nflow = new Flow();

            nflow.desc = this.desc;
            nflow.configRepeat = this.configRepeat;
            nflow.repeated = this.repeated;

            foreach ( var act in acts)
            {
                nflow.acts.Add(act.Clone(agent));
            }

            return nflow;
        }

        /// <summary>
        ///  clone and bind agent
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public Flow Clone(Agent agent)
        {
            var flow = Clone();

            flow.Bind(agent);

            return flow;
        }

        public void Start()
        {
            index = 0;

            acts[this.index].Begin();

            foreach ( var act in globalActs )
            {
                act.Begin();
            }
        }

        public void Do()
        {
            Contract.Assert(index >= 0);
            Contract.Assert(index < acts.Count);

            if (acts[index].IsInBeginState)
            {
                acts[index].Do();
            }

            foreach (var act in globalActs)
            {
                if (act.IsInBeginState )
                {
                    act.Do();
                }
            }
        }

        public void On(Msg m)
        {
            Contract.Assert(index >= 0);
            Contract.Assert(index < acts.Count);

            if (acts[index].IsInBeginState)
            {
                acts[index].On(m);
            }

            foreach (var act in globalActs)
            {
                if (act.IsInBeginState)
                {
                    act.On(m);
                }
            }
        }

        public void Next()
        {
            acts[index].End();

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

                    return;
                }
            }

            acts[index].Begin();
        }

        public void Jump(int jumpIndex)
        {
            acts[this.index].End();

            int absIndex = jumpIndex < 0 ? this.index + jumpIndex : this.index + jumpIndex;

            if (absIndex >= 0 && absIndex < acts.Count)
            {
                this.index = absIndex;

                acts[this.index].Begin();
            }
            else
            {
                throw new ArgumentOutOfRangeException($"index is out of range {index}");
            }
        }

        public void Jump(string act)
        {
            for( int i=0; i<acts.Count; ++i)
            {
                if ( acts[i].Name == act )
                {
                    this.index = i;
                    acts[this.index].Begin();

                    return;
                }
            }

            logger.Error($"Jump. act: {act} not found");
        }

        public void Complete()
        {
            if ( index >= 0 && index < acts.Count)
            {
                acts[index].End();
            }

            foreach ( var act in globalActs )
            {
                act.End();
            }
        }
    }
}
