using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loady
{
    public class Loop
    {
        List<object> objects = new List<object>();

        int index = 0;

        public int Index { get { return index; } }

        public Loop()
        {
        }

        public void Add(object obj)
        {
            objects.Add(obj);
        }

        public object Next()
        {
            if ( index < objects.Count)
            {
                return objects[index++];
            }

            return null;
        }

        public bool IsEnd()
        {
            return index == objects.Count;
        }
    }
}
