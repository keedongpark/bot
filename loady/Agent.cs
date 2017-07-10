using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using NLog;

namespace loady
{

    public class Agent
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();

        int index = -1;
        Flow flow;
        bool completed = false;
        bool failed = false;
        string msg = "";
        bool delayed = false;
        int delayedMilliSeconds = 0;
        int delayStartTick = 0;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public class Config
        {
            public string id = "test";
            public string pw = "test";
        }

        Config config;

        /// <summary>
        /// get index 
        /// </summary>
        public int Index { get { return index; } }

        /// <summary>
        /// 실패 여부 
        /// </summary>
        public bool Failed { get { return failed; } }

        /// <summary>
        /// 완료 여부
        /// </summary>
        public bool IsCompleted { get { return completed; } }

        /// <summary>
        /// 완료 메세지
        /// </summary>
        public string Message { get { return msg; } }

        public Flow Flow { get { return flow; } }

        /// <summary>
        /// 테스트 용도로만 사용
        /// </summary>
        public Agent()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="def"></param>
        public Agent(int index, Config config)
        {
            this.index = index;
            this.config = config;
        }

        public void Set(Flow flow)
        {
            this.flow = flow;
        }

        public void Start()
        {
            flow.Start();
        }

        public void Execute()
        {
            if ( IsCompleted )
            {
                return;
            }

            // TODO: process network for this agent

            try
            {
                if (delayed)
                {
                    if ((Environment.TickCount - delayStartTick) < delayedMilliSeconds)
                    {
                        return;
                    }

                    delayed = false;
                    delayStartTick = 0;
                }

                flow.Do();
            }
            catch (Exception ex)
            {
                logger.Error(ex);

                Complete(true, ex.ToString());
            }
        }

        public void Complete(bool fail, string msg)
        {
            failed = fail;
            completed = true;
            this.msg = msg;
        }

        public void next()
        {
            flow.Next();
        }

        public void jump(int index)
        {
            flow.Jump(index);
        }

        public void fail(string msg = "")
        {
            Complete(true, $"fail from script w/ {msg}");
        }

        public void delay(int milliseconds)
        {
            delayed = true;
            delayedMilliSeconds = milliseconds;
            delayStartTick = Environment.TickCount;
        }

        public bool set(string key, bool v)
        {
            dict[key] = v;

            return v;
        }

        public object set(string key, object v)
        {
            dict[key] = v;
            return v;
        }

        public object has(string key)
        {
            return dict.ContainsKey(key);
        }

        public bool check(string key)
        {
            if (dict.ContainsKey(key))
            {
                return (bool)dict[key];
            }

            return false;
        }

        public object get(string key)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            return null;
        }

        public string get_string(string key)
        {
            return get(key) as string;
        }

        public float get_float(string key)
        {
            return (float)get(key);
        }

        public int get_int(string key)
        {
            return (int)get(key);
        }

        public void clear(string key)
        {
            dict.Remove(key);
        }
    }
}
