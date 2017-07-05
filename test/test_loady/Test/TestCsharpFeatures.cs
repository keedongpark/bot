using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace test_loady.Test
{
    [TestFixture]
    class TestCsharpFeatures
    {
        [Test]
        public void TestImplicitObjectConversion()
        {
            object obj = "hello";

            Assert.IsTrue(obj as string == "hello");

            // explicit casting is required to compare value
        }

        [Test]
        public void TestVariadicFunction()
        {
            CallEpsilon("a", "b", 3, 5); 
        }

        [Test]
        public void TestObjectUse()
        {
            v = "a";
            Assert.IsTrue(v as string == "a");

            v = 1;
            Assert.IsTrue(Convert.ToInt32(v) == 1);
        }

        void CallEpsilon(params object[] objs)
        {

        }

        object v;
    }
}
