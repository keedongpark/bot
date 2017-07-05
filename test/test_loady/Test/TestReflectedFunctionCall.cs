using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using NUnit.Framework;

namespace test_loady.Test
{
    class Callee
    {
        public void CallMe(string arg1)
        {
            v = arg1;
        }

        public string v = "None";
    }

    [TestFixture]
    class TestReflectedFunctionCall
    {
        [Test]
        public void TestCallFunction()
        {
            var callee = new Callee();
            var assembly = Assembly.GetExecutingAssembly();

            Type type = assembly.GetType("test_loady.Test.Callee");

            if (type != null)
            {
                MethodInfo methodInfo = type.GetMethod("CallMe");

                if (methodInfo != null)
                {
                    object result = null;

                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    object[] parametersArray = new object[] { "Hello" };

                    result = methodInfo.Invoke(callee, parametersArray);
                } 
            }

            Assert.IsTrue(callee.v == "Hello");
        }
    }
}
