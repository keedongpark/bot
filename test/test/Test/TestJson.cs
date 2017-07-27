using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace test.Test
{
    [TestFixture]
    class TestJson
    {
        [Test]
        public void TestJsonParse()
        {
            var tok = JObject.Parse(json);
            var err = tok["error"];

            Assert.IsTrue(err.Value<int>() == 0);
            Assert.IsTrue(tok["res"][0]["ri"].Value<int>() == 1);
        }

        [Test]
        public void TestJsonAdd()
        {
            var tok = JToken.Parse(json);

            tok["prop"] = "value";

            Assert.IsTrue(tok["prop"].Value<string>() == "value");

            System.Console.WriteLine(tok.ToString());

            // 그냥 참조해서 추가하면 된다. 

            // AddAfterSelf(new JProperty(...)) 로 추가하려면 좀 더 맞춰야 한다. 

            // http://www.newtonsoft.com/json/help/html/ModifyJson.htm
            // - 자세한 예시들이 있다.  
        }

        [Test]
        public void TestJsonAddArray()
        {
            var tok = JToken.Parse(json);

            tok["prop"] = "value";

            var users = new JArray();
            users.Add(1);
            users.Add(2);

            foreach( var child in users )
            {

            }


            tok["users"] = users;

            Assert.IsTrue(tok["prop"].Value<string>() == "value");

            System.Console.WriteLine(tok.ToString());

            // 그냥 참조해서 추가하면 된다. 

            // AddAfterSelf(new JProperty(...)) 로 추가하려면 좀 더 맞춰야 한다. 

            // http://www.newtonsoft.com/json/help/html/ModifyJson.htm
            // - 자세한 예시들이 있다.  
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
