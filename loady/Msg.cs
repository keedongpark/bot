using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace loady
{
    public class Msg
    {
        public JToken json = JToken.Parse("{}");

        public bool Set(string m)
        {
            json = JToken.Parse(m);

            return true;
        }

    }
}
