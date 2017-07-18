using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using System.IO;
using NLog;
using Newtonsoft.Json.Linq;

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
        int executeCount = 0;
        string typeName = "loady.Agent";
        Session session;
        ConcurrentQueue<Msg> recvQ = new ConcurrentQueue<Msg>();

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

        /// <summary>
        /// get flow
        /// </summary>
        public Flow Flow { get { return flow; } }

        /// <summary>
        /// get execute count
        /// </summary>
        public int ExecuteCount { get { return executeCount; } }

        /// <summary>
        /// get session
        /// </summary>
        public Session Session { get { return session; } }

        /// <summary>
        /// get recv queue
        /// </summary>
        public ConcurrentQueue<Msg> RecvQueue { get { return recvQ; } }

        /// <summary>
        /// 테스트 용도로만 사용
        /// </summary>
        public Agent()
        {
        }

        /// <summary>
        /// Agent의 기본 데이터를 설정 
        /// Effect: index, config가 지정됨
        /// </summary>
        /// <param name="index"></param>
        /// <param name="def"></param>
        public Agent(int index, Config config)
        {
            this.index = index;
            this.config = config;
            this.session = new Session(this);

            SetTypeName(nameof(Agent));            

            Contract.Assert(this.index >= 0);
            Contract.Assert(this.config.id.Length > 0);
            Contract.Assert(this.config.pw.Length > 0);
        }

        /// <summary>
        /// Flow를 지정. 
        /// </summary>
        /// <param name="flow"></param>
        public void Set(Flow flow)
        {
            this.flow = flow;

            Contract.Assert(this.flow != null);
            Contract.Assert(this.flow.Acts.Count > 0);
        }

        /// <summary>
        /// Set typename (namespace.class) 
        /// call() 함수가 동작하려면 실제 타잎이 필요
        /// </summary>
        /// <param name="typeName"></param>
        public void SetTypeName(string typeName)
        {
            this.typeName = typeName;
        }

        /// <summary>
        /// Flow를 시작. 
        /// Effect: Flow의 시작 액트로 이동
        /// </summary>
        public void Start()
        {
            Contract.Assert(flow != null);

            flow.Start();

            OnStart();
        }

        /// <summary>
        /// 실행. 완료되었으면 아무것도 안 함. 
        /// Flow를 실행. 스크립트 실행됨
        /// Pre: Constructor / Set / Start called 
        /// Post: 스크립트 실행됨. 메세지 있으면 처리됨
        /// </summary>
        public void Execute()
        {
            Contract.Assert(flow != null);
            Contract.Assert(index >= 0);

            if ( IsCompleted )
            {
                return;
            }

            Msg m;

            // process messages first
            while (recvQ.TryDequeue(out m))
            {
                flow.On(m);
            }

            Contract.Assert(!IsCompleted);

            ++executeCount;

            OnExecute();

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
                logger.Error($"Agent exception: {ex}");

                Complete(true, ex.ToString());
            }
        }

        public void Complete(bool fail, string msg)
        {
            failed = fail;
            completed = true;
            this.msg = msg;

            flow.Complete();

            OnComplete();

            logger.Info($"Completed {Index}");
        }

        public Msg Peek()
        {
            Msg m;

            if( recvQ.TryPeek(out m) )
            {
                return m;
            }

            return null;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="stream"></param>
        public void Recv(MemoryStream stream)
        {
            OnRecv(stream);

            // 처음으로 이동 시킴
            stream.Seek(0, SeekOrigin.Begin);
        }

        protected virtual void OnRecv(MemoryStream stream)
        {
            // parse and push msg into queue
        }

        protected virtual void OnSend(Msg m)
        {
            // make into bytes and send it
        }

        #region Override Functions 
        protected virtual void OnStart()
        {

        }

        protected virtual void OnExecute()
        {
        
        }

        protected virtual void OnExecuteNet()
        {
            
        }

        protected virtual void OnComplete()
        {

        }
        #endregion

        /// <summary>
        /// Def: Script Functions 
        /// - are functions that can be called from scripts 
        /// - can be used only when Execute() can be called 
        /// </summary>
        #region Script Functions

        public void next()
        {
            flow.Next();
        }

        /// <summary>
        /// jump to index. 
        /// </summary>
        /// <param name="index">현재 인덱스에서 상대적인 값</param>
        public void jump(int index)
        {
            flow.Jump(index);
        }

        public void fail(string msg = "")
        {
            Complete(true, $"fail from script w/ {msg}");
        }

        public void complete(string msg="")
        {
            Complete(false, msg);
        }

        public object call(string method, params object[] args)
        {
            var assembly = Assembly.GetExecutingAssembly();

            try
            {
                Type type = assembly.GetType(this.typeName);

                MethodInfo methodInfo = type.GetMethod(method);

                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    return methodInfo.Invoke(this, args);
                }
            }
            catch ( Exception ex)
            {
                logger.Error(ex);

                fail($"method {method} call failed");
            }

            return null;
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

        public JToken get_json(string key)
        {
            return (JToken)get(key);
        }

        public void clear(string key)
        {
            dict.Remove(key);
        }

        public void connect(string ip, ushort port)
        {
            Session.Connect(ip, port);
        }

        public void send(loady.Msg m)
        {
            OnSend(m);
        }

        public bool is_connected()
        {
            return Session.IsConnected();
        }

        public void disconnect()
        {
            Session.Disconnect();
        }
        #endregion
    }
}


