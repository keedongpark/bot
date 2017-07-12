using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NUnit.Framework;
using YamlDotNet.RepresentationModel;

namespace test.Test
{
    [TestFixture]
    class TestYamlConfig
    {
        [Test]
        public void TestParseYaml()
        {
            var input = new StringReader(doc);

            // Load the stream
            var yaml = new YamlStream();
            yaml.Load(input);

            // Examine the stream
            var mapping = (YamlSequenceNode)yaml.Documents[0].RootNode;

            Assert.IsTrue(mapping != null);

            // 첫 자식이 mapping 노드이다. 
            var flow_map = (YamlMappingNode)mapping.Children[0];
            var flow = (YamlMappingNode)flow_map.Children["flow"];

            // 이름으로 scalar 노드를 얻는다.
            var name = (YamlScalarNode)flow.Children["name"];


            // 접근: 
            // - SequenceNode는 Children[index]로 접근 
            // - MappingNode는 ScalarNode(value)로 접근
        }

        [Test]
        public void TestLoadyConfig()
        {

        }

        private string doc = @"
- flow: 
   name: login 
   acts: 
    - act: 
        name: check_connected
        type: cond 
        do: agent.is_connected() 

    - act: 
        type: exec
        do: agent.exec(""send_login"", parms)

    - act: 
        type: wait_msg
        timeout: 3
        do: > 
          if ( msg.get_key() == ""login_resp"" )
          {
            if ( msg.get(""result"") == true )
            {
              agent.set(""is_logined"", true);
            }
            else 
            {
              agent.failed();
            }
          }

    - act: 
        type: cond
        do: > 
          agent.is_logined()

";

    }
}
