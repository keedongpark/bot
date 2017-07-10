using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using loady;

namespace test_loady.Test
{
    [TestFixture]
    class TestAgent
    {

        [Test]
        public void TestConfig()
        {
            var loader = new Loader();

            Assert.IsTrue(loader.Load("test_pass.yaml"));
            Assert.IsTrue(loader.Agents.Count == 100);
            Assert.IsTrue(loader.Agents[0].Flow.Acts.Count == 2);
            Assert.IsTrue(loader.Agents[2].Flow.Acts.Count == 1);
            Assert.IsTrue(loader.Agents[2].Flow.Acts[0].HasDoScript);
            Assert.IsTrue(loader.Agents[2].Flow.Acts[0].HasOnScript == false);
        }

        [Test]
        public void TestDo()
        {

        }

        [Test]
        public void TestOn()
        {

        }

        [Test]
        public void TestDict()
        {
            var agent = new loady.Agent();

            agent.set("v1", true);
            Assert.IsTrue(agent.check("v1"));

            // exception on check with v2 key
            // agent.set("v2", "true");

            // 위와 같이 unboxing 할 때 exception 나는 걸 피하려면 타잎별 dict 필요. 
            // 일단 진행하고 불편해 지면 추가한다.
        }
    }
}
