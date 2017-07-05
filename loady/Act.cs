using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace loady
{

    // Executes simulation
    // - process timeout 
    // - process repetition
    public abstract class Act
    {
        private List<Act> children = null;
        private int currentChild = 0;
        private YamlMappingNode def = null;
        private Agent agent = null;

        public enum Result
        {
            SucessStop, 
            SucessContinue,
            FailStop, 
            FailContinue, 
        }

        // TODO: 
        // Dictionary for Act

        public Act Current
        {
            get
            {
                if ( children == null || children.Count == 0)
                {
                    return null;
                }

                return children[currentChild];
            }
        }

        public Agent Agent { get { return agent; } }

        public Act(Agent owner, YamlMappingNode def)
        {
            this.agent = owner;
            this.def = def;

            // TODO: 
            // Build Name / Type 
        }

        virtual public Result Execute()
        {
            // Execute
            return Result.SucessContinue;
        }

        virtual public Result Execute(Msg m)
        {
            return Result.SucessContinue;
        }
    }

    public class Flow : Act
    {
        public Flow(Agent owner, YamlMappingNode def)
            : base(owner, def)
        {
        }
    }
}
