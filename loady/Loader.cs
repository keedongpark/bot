﻿using System;
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
        List<Runner> runners = new List<Runner>();
        Flow sharedFlow;

        int runnerCount = 1;
        string accountPrefix = "test_";
        string passwordPrefix = "test_";
        int beginIndex = 0;
        int agentCount = 1;
        Module module;

        Logger logger = LogManager.GetCurrentClassLogger();

        public List<Agent> Agents { get { return agents; } }

        public Loader(Module module)
        {
            this.module = module;

            Contract.Assert(module != null);
        }

        public bool Load(string filename)
        {
            StreamReader sr = new StreamReader(filename);

            // http://www.yamllint.com/
            // to verify 
            // - replacing tab with spaces is safe mostly.
            // - keep indentation same for each key 

            var yaml = new YamlStream();
            yaml.Load(sr);

            var root = (YamlMappingNode)yaml.Documents[0].RootNode;

            // load flow
            var flowNode = (YamlMappingNode)root.Children["flow"];
            sharedFlow = new loady.Flow(flowNode);

            // load agents
            var agentsNode = (YamlMappingNode)root.Children["agents"];
            var accountPrefixNode = (YamlScalarNode)agentsNode.Children["account_prefix"];
            var passwordPrefixNode = (YamlScalarNode)agentsNode.Children["password_prefix"];
            var beginNode  = (YamlScalarNode)agentsNode.Children["begin"];
            var countNode  = (YamlScalarNode)agentsNode.Children["count"];
            var runnersNode = (YamlScalarNode)agentsNode.Children["runners"];

            accountPrefix = accountPrefixNode.Value;
            passwordPrefix = passwordPrefixNode.Value;
            beginIndex = Int32.Parse(beginNode.Value);
            agentCount = Int32.Parse(countNode.Value);
            runnerCount = Int32.Parse(runnersNode.Value);

            CreateAgents(agentsNode);
            CreateRunners();

            return Agents.Count > 0;
        }

        public bool LoadMod(string filename)
        {
            return Mods.Inst().Load(filename);
        }

        public void Start()
        {
            foreach (var runner in runners)
            {
                runner.Start();
            }
        }

        public void Wait()
        {
            bool completed = false;

            while ( !completed )
            {
                completed = true; // try to exit

                foreach (var runner in runners)
                {
                    if (!runner.IsCompleted())
                    {
                        completed = false; // prevent exit
                        break; 
                    }
                }

                System.Threading.Thread.Sleep(100);
            }
        }

        public void Stop()
        {
            foreach (var runner in runners)
            {
                runner.Stop();
            }
        }

        void CreateAgents(YamlMappingNode agentsNode)
        {
            for ( int i=0; i<agentCount; ++i)
            {
                var agent = CreateAgent(i, agentsNode);
                agents.Add(agent);
            }
        }

        void CreateRunners()
        {
            for ( int i=0; i<runnerCount; ++i)
            {
                runners.Add(new Runner());
            }

            for ( int i=0; i<runnerCount; ++i)
            {
                foreach ( var agent in agents )
                {
                    var idx = agent.Index % runnerCount;
                    runners[idx].Add(agent);
                }
            }
        }

        Agent CreateAgent(int index, YamlMappingNode agentsNode)
        {
            var agentKey = $"agent_{index}";
            var config = new Agent.Config();

            var idx = index.ToString("000");

            config.id = $"{accountPrefix}{idx}";
            config.pw = $"{passwordPrefix}{idx}";

            if ( agentsNode.Children.ContainsKey(agentKey) )
            {
                var agentNode = (YamlMappingNode)agentsNode.Children[agentKey];
                var flowNode = (YamlMappingNode)agentNode.Children["flow"];
                var flow = new Flow(flowNode);

                var agent1 = module.Create(index, config);
                agent1.Set(flow);

                return agent1;
            }

            var agent = module.Create(index, config);
            agent.Set(sharedFlow.Clone(agent));

            return agent;
        }
    }
}
