using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using YamlDotNet.RepresentationModel;
using NLog; 

namespace loady
{
    public class Loader
    {
        List<Agent> agents = new List<Agent>();
        List<Runner> runers = new List<Runner>();
        Flow sharedFlow;

        int runnerCount = 1;
        string prefix = "test_";
        int beginIndex = 0;
        int agentCount = 1;
        string module = "";

        Logger logger = LogManager.GetCurrentClassLogger();

        public List<Agent> Agents { get { return agents; } }

        public bool Load(string filename)
        {
            StreamReader sr = new StreamReader(filename);

            var yaml = new YamlStream();
            yaml.Load(sr);

            var root = (YamlMappingNode)yaml.Documents[0].RootNode;

            // load flow
            var flowNode = (YamlMappingNode)root.Children["flow"];
            sharedFlow = new loady.Flow(flowNode);

            // load agents
            var agentsNode = (YamlMappingNode)root.Children["agents"];
            var prefixNode = (YamlScalarNode)agentsNode.Children["prefix"];
            var beginNode  = (YamlScalarNode)agentsNode.Children["begin"];
            var countNode  = (YamlScalarNode)agentsNode.Children["count"];
            var runnersNode = (YamlScalarNode)agentsNode.Children["runners"];
            var moduleNode = (YamlScalarNode)agentsNode.Children["module"];

            prefix = prefixNode.Value;
            beginIndex = Int32.Parse(beginNode.Value);
            agentCount = Int32.Parse(countNode.Value);
            runnerCount = Int32.Parse(runnersNode.Value);
            module = moduleNode.Value;

            CreateAgents(agentsNode);

            return Agents.Count > 0;
        }

        public void Start()
        {
            // starts all agents
        }

        public void Stop()
        {
            // stop all agents
        }

        void CreateAgents(YamlMappingNode agentsNode)
        {
            for ( int i=0; i<agentCount; ++i)
            {
                var agent = CreateAgent(i, agentsNode);
                agents.Add(agent);
            }
        }

        Agent CreateAgent(int index, YamlMappingNode agentsNode)
        {
            var agentKey = $"agent_{index}";
            var config = new Agent.Config();

            config.id = $"{prefix}{index}";
            config.pw = $"{prefix}{index}";

            if ( agentsNode.Children.ContainsKey(agentKey) )
            {
                var flowNode = (YamlMappingNode)agentsNode.Children["flow"];
                var flow = new Flow(flowNode);

                var agent1 = new loady.Agent(index, config);
                agent1.Set(flow);

                return agent1;
            }

            var agent = new loady.Agent(index, config);
            agent.Set(sharedFlow.Clone(agent));

            return agent;
        }
    }
}
