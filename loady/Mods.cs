using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace loady
{
    class Mods
    {
        private static Mods mods = new Mods();

        public static Mods Inst()
        {
            return mods;
        }

        private Mods()
        {
        }

        private Dictionary<string, Act> acts = new Dictionary<string, Act>();

        public bool Load(string filename)
        {
            StreamReader sr = new StreamReader(filename);

            var yaml = new YamlStream();
            yaml.Load(sr);

            string modName = Path.GetFileNameWithoutExtension(filename);

            var rootNode = (YamlMappingNode)yaml.Documents[0].RootNode;
            var actsNode = (YamlSequenceNode)rootNode.Children["acts"];

            for ( int i=0; i<actsNode.Children.Count; ++i )
            {
                Load(modName, (YamlMappingNode)actsNode.Children[i]);
            }

            if (rootNode.Children.ContainsKey("funcs"))
            {
                var funcsNode = (YamlSequenceNode)rootNode.Children["funcs"];

                for (int i = 0; i < funcsNode.Children.Count; ++i)
                {
                    var funcNode = (YamlMappingNode)funcsNode.Children[i];
                    var funcBody = funcNode.Children["func"];

                    LoadFunc(modName, ((YamlScalarNode)funcBody).Value);
                }
            }

            return true;
        }

        public Act Get(string mod, string name)
        {
            string fullName = $"{mod}.{name}";

            return Get(fullName);
        }

        public Act Get(string fullName )
        {
            if ( acts.ContainsKey(fullName))
            {
                return acts[fullName];
            }

            return null;
        }

        public bool Has(string mod, string name)
        {
            return acts.ContainsKey($"{mod}.{name}");
        }

        public bool Has(string fullName)
        {
            return acts.ContainsKey(fullName);
        }

        private bool Load(string mod, YamlMappingNode actDef)
        {
            var act = new Act(0, actDef);

            return Add(mod, act);
        }

        private bool Add(string mod, Act act)
        {
            if ( act == null )
            {
                return false;
            }
           
            if ( act.Name.Length == 0)
            {
                return false;
            }

            if ( Has(mod, act.Name) )
            {
                return false;
            }

            acts.Add($"{mod}.{act.Name}", act);

            return true;
        }

        private void LoadFunc(string mod, string func)
        {
            Builder.Inst().LoadFunc(mod, func);
        }
    }
}
