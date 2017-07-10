using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NUnit.Framework;
using loady;
using YamlDotNet.RepresentationModel;

namespace test_loady.Test
{
    [TestFixture]
    class TestAct
    {
        [Test]
        public void TestActDo()
        {
            var agent = new loady.Agent();
            var input = new StringReader(def1);

            var yaml = new YamlStream();
            yaml.Load(input);

            var def = (YamlMappingNode)yaml.Documents[0].RootNode;
            var act1 = new Act(0, agent, def);

            Assert.IsTrue(agent == act1.Agent);
            Assert.IsTrue(act1.HasDoScript);
            Assert.IsTrue(act1.HasOnScript);

            Assert.IsTrue(act1.Do() == Act.Result.SucessContinue);
        }

        [Test]
        public void TestActOn()
        {

        }

        const string def1 = @"
         act: 
                do :  > 
                    if ( agent.check(""connected"") )
                    {
                        agent.next();
                    }
                    else 
                    {
                        agent.fail();
                    }
                on:  > 
                    if ( agent.check(""connected"") )
                    {
                        agent.next();
                    }
                    else 
                    {
                        agent.fail();
                    }
";
    }
}
