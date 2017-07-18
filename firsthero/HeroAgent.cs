using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace firsthero
{
    public class HeroAgent : loady.Agent
    {
        // 통신 처리 구조 구현
        //  - SuperSocket 사용해 보기 
        //  - SuperSocket 연결하기 
        //  - Firsthero 프로토콜 구현하기 

        public HeroAgent(int index, Config config)
            : base(index, config)
        {
        }

        protected override void OnRecv(MemoryStream stream)
        {
            // parse protocol
            // push msg into queue
        }

        protected override void OnStart()
        {
        }

        protected override void OnExecute()
        {
        }

        protected override void OnExecuteNet()
        {
            // pull from queue and call on function
        }

        protected override void OnComplete()
        {
        }
    }
}
