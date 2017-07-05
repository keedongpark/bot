using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace test_loady.Test
{
    [TestFixture]
    class TestJson
    {
        [Test]
        public void TestJsonParse()
        {
            var tok = JToken.Parse(json);
            var err = tok["error"];

            Assert.IsTrue(err.Value<int>() == 0);
            Assert.IsTrue(tok["res"][0]["ri"].Value<int>() == 1);
        }

        const string json = @"
{
 ""error"":0,
 ""ts"":90,
 ""res"":[
 {
 ""ri"":1,
 ""ru"":400,
 ""rr"":84420
 }
 ],
 ""me"":100
 }
";
    }
}
