using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace loady
{
    public class ActExec : Act
    {
        public ActExec(Agent owner, YamlMappingNode def)
            : base(owner, def)
        {
        }

        public override Result Execute()
        {
            return base.Execute();
        }

        public override Result Execute(Msg m)
        {
            return base.Execute(m);
        }
    }
}
