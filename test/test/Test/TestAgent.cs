using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using loady;

namespace test.Test
{
    [TestFixture]
    class TestAgent
    {

        [Test]
        public void TestConfig()
        {
            var loader = new Loader(new DefaultModule());

            var path = @"c:\projects\bot\test\test\Test\";

            Assert.IsTrue(loader.Load($"{path}test_pass.yaml"));
            Assert.IsTrue(loader.Agents.Count == 100);
            Assert.IsTrue(loader.Agents[0].Flow.Acts.Count == 2);
            Assert.IsTrue(loader.Agents[2].Flow.Acts.Count == 1);
            Assert.IsTrue(loader.Agents[2].Flow.Acts[0].HasDoScript);
            Assert.IsTrue(loader.Agents[2].Flow.Acts[0].HasOnScript == false);
        }

        [Test]
        public void TestDo()
        {
            var loader = new Loader(new DefaultModule());

            var path = @"c:\projects\bot\test\test\Test\";

            Assert.IsTrue(loader.Load($"{path}test_pass.yaml"));
            Assert.IsTrue(loader.Agents.Count == 100);
            Assert.IsTrue(loader.Agents[0].Flow.Acts.Count == 2);

            var agent = loader.Agents[0];
            var act = agent.Flow.Acts[0];

            agent.Start();
            agent.Execute();

            Assert.IsTrue(agent.ExecuteCount == 1);
            Assert.IsTrue(agent.Flow.Index == 1);

            agent.Execute();

            Assert.IsTrue(agent.ExecuteCount == 2);
            Assert.IsTrue(agent.Flow.Index == 0);

            agent.Complete(false, "Finished");
        }

        [Test]
        public void TestDoFail()
        {
            var loader = new Loader(new DefaultModule());

            var path = @"c:\projects\bot\test\test\Test\";

            Assert.IsTrue(loader.Load($"{path}test_pass.yaml"));
            Assert.IsTrue(loader.Agents.Count == 100);
            Assert.IsTrue(loader.Agents[0].Flow.Acts.Count == 2);

            var agent = loader.Agents[0];
            var act = agent.Flow.Acts[0];

            agent.Start();
            agent.Execute();

            Assert.IsTrue(agent.ExecuteCount == 1);
            Assert.IsTrue(agent.Flow.Index == 1);

            agent.Execute();

            Assert.IsTrue(agent.ExecuteCount == 2);
            Assert.IsTrue(agent.Flow.Index == 0);

            agent.Complete(false, "Finished");
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
