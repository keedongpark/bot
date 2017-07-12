using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using NUnit.Framework;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace test_loady.Test
{
    public class TestAgentDummy
    {
        public void call(string typeName, string method, params object[] args)
        {
            var assembly = Assembly.GetExecutingAssembly();

            Type type = assembly.GetType(typeName);

            if (type != null)
            {
                MethodInfo methodInfo = type.GetMethod(method);

                if (methodInfo != null)
                {
                    object result = null;

                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    result = methodInfo.Invoke(this, args);
                }
            }
        }
    }

    public class TestAgentSub : TestAgentDummy
    {
        public void Hello()
        {
        }

        public void HelloParams(int i, float f, string s)
        {
            iv = i;
            fv = f;
            sv = s;
        }

        public int iv;
        public float fv;
        public string sv;
    }
    public class Globals
    {
        public int value = 0;

        public TestAgentDummy agent;
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
            var global = new Globals();
            var script = "value = 5";

            var state = CSharpScript.RunAsync(script, globals: global);
            state.Wait();

            Assert.IsTrue(global.value == 5);
        }

        [Test]
        public void RunWithCompiledScript()
        {
            var script = CSharpScript.Create<bool>("0 > 3", globalsType: typeof(Globals));
            script.Compile();

            var res = script.RunAsync(new Globals());
            Assert.IsTrue(res.Result.ReturnValue == false);
        }

        [Test]
        public void CheckPerfOfCompiledScript()
        {
            var script = CSharpScript.Create("value = 3", globalsType: typeof(Globals));
            script.Compile();

            // 여기서는 parse tree 정도까지만 만듦
            // RunAsync에서 실제 참조 등에 대한 처리가 이루어짐

            var global = new Globals();

            const int testCount = 10000;
            
            for (int i = 0; i < testCount; ++i)
            {
                var res = script.RunAsync(global);
                Assert.IsTrue(global.value == 3);
            }

            // 디버그 / 릴리스:  
            // - 1만번 실행에 2초. 한번 실행도 2초. 
            // - 컴파일은 느리지만 간단한 실행은 느리지 않다.   
            // 
            // 컴파일 되는 오브젝트들을 한번에 로딩하고 사용하도록 해야 함
            // 디버깅이 쉬워야 함
        }

        [Test]
        public void TestScriptSubClass()
        {
            var script = CSharpScript.Create(
                @"agent.call(""test_loady.Test.TestAgentSub"", ""Hello"");",
                globalsType: typeof(Globals)
                );

            script.Compile();

            var global = new Globals();
            global.agent = new TestAgentSub();

            var res = script.RunAsync(global);

            // 
            // subclass에 대해 직접 호출하는 건 당연히 안 된다. 
            // reflection을 통해 type을 알려주고 Invoke하면 된다. 
        } 

        [Test]
        public void TestScriptCallWithParams()
        {
            var script = CSharpScript.Create(
                @"agent.call(""test_loady.Test.TestAgentSub"", ""HelloParams"", 1, 1.0f, ""string"");",
                globalsType: typeof(Globals)
                );

            script.Compile();

            var global = new Globals();
            global.agent = new TestAgentSub();

            var res = script.RunAsync(global);

            var sub = (TestAgentSub)global.agent;

            Assert.IsTrue(sub.iv == 1);
            Assert.IsTrue(sub.sv == "string");

            // 내부적으로 타잎 자동 변환. 함수 구현 차이는 없음. 
            // Invoke의 성능 자체는 느릴 듯. 
        } 
    }
}
