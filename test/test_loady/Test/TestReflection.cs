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

    class CalleeChild : Callee
    {
        public void CallChild(string arg1)
        {
            c = arg1;
        }

        public string c = "None";
    }

    [TestFixture]
    class TestReflection
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

        [Test]
        public void TestSubclassReflection()
        {
            var callee = new CalleeChild();
            var assembly = Assembly.GetExecutingAssembly();

            Callee parent = callee;

            Type type = assembly.GetType("test_loady.Test.CalleeChild");

            if (type != null)
            {
                MethodInfo methodInfo = type.GetMethod("CallChild");

                if (methodInfo != null)
                {
                    object result = null;

                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    object[] parametersArray = new object[] { "Hello" };

                    result = methodInfo.Invoke(parent, parametersArray);
                }
            }

            Assert.IsTrue(callee.c == "Hello");
        }
    }
}
