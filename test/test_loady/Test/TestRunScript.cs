using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace test_loady.Test
{
    public class Agent
    {
        public int value = 0;
    }

    [TestFixture]
    class TestRunScript
    {
        [Test]
        public void RunCodeAsScript()
        {
            // requires .net 4.6+ for rosyln
            // 많은 것들이 깔린다. 

            var script = "3 == 3";

            var res = CSharpScript.EvaluateAsync<bool>(script);
            res.Wait();

            Assert.IsTrue(res.Result);

            // https://daveaglick.com/posts/roslyn-based-dsls-vs-standard-csharp-scripts
            // 일반적인 아이디어. 사용법. 자랑.

            // https://github.com/dotnet/roslyn/wiki/Scripting-API-Samples 
            // scripting 사용예. 글로벌 지정. Continue

        }

        [Test]
        public void RunWithGlobalPassed()
        {
            var agent = new Agent();
            var script = "value = 5";

            var state = CSharpScript.RunAsync(script, globals: agent);
            state.Wait();

            Assert.IsTrue(agent.value == 5);
        }

        [Test]
        public void RunWithCompiledScript()
        {
            var script = CSharpScript.Create<bool>("0 > 3", globalsType: typeof(Agent));
            script.Compile();

            var res = script.RunAsync(new Agent());
            Assert.IsTrue(res.Result.ReturnValue == false);
        }

        [Test]
        public void CheckPerfOfCompiledScript()
        {
            var script = CSharpScript.Create("value = 3", globalsType: typeof(Agent));
            script.Compile();

            // 여기서는 parse tree 정도까지만 만듦
            // RunAsync에서 실제 참조 등에 대한 처리가 이루어짐

            var agent = new Agent();

            const int testCount = 10000;
            
            for (int i = 0; i < testCount; ++i)
            {
                var res = script.RunAsync(agent);
                Assert.IsTrue(agent.value == 3);
            }

            // 디버그 / 릴리스:  
            // - 1만번 실행에 2초. 한번 실행도 2초. 
            // - 컴파일은 느리지만 간단한 실행은 느리지 않다.   
            // 
            // 컴파일 되는 오브젝트들을 한번에 로딩하고 사용하도록 해야 함
            // 디버깅이 쉬워야 함
        }
    }
}
