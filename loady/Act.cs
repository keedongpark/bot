using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

using NLog;

namespace loady
{

    // Executes simulation
    // - process timeout 
    // - process repetition
    public class Act
    {
        public enum Result
        {
            SucessStop,
            SucessContinue,
            FailStop,
            FailStopException, 
            FailContinue,
        }

        // script에서 접근 
        public class Globals
        {
            public Agent agent;
            public Msg msg;
        }

        public enum State
        {
            None, 
            Begin, 
            End 
        }

        // 기본 필드들
        private int index = -1;
        private YamlMappingNode def = null;
        private Agent agent = null;

        // 스크립트 관련 필드
        private Globals globals = new Globals();

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private State state = State.None;
        private DateTime beginTime;
        private DateTime endTime;

        /// <summary>
        /// get Agent
        /// </summary>
        public Agent Agent { get { return agent; } }

        /// <summary>
        /// get index in a flow 
        /// </summary>
        public int Index {  get { return index; } }

        /// <summary>
        /// 시작 스크립트를 가짐 
        /// </summary>
        public bool HasBeginScript { get; private set; }

        /// <summary>
        /// 종료 스크립트를 가짐
        /// </summary>
        public bool HasEndScript { get; private set; }

        /// <summary>
        /// 실행 스크립트를 가짐
        /// </summary>
        public bool HasDoScript { get; private set; }

        /// <summary>
        /// msg 처리 스크립트를 가짐 
        /// </summary>
        public bool HasOnScript { get; private set; }

        /// <summary>
        /// Has Begun  
        /// </summary>
        public bool IsInBeginState { get { return state == State.Begin; } }

        /// <summary>
        /// Get name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// To copy
        /// </summary>
        private Act()
        {
        }

        /// <summary>
        /// 초기화 
        /// </summary>
        /// <param name="owner">나를 실행하는 에이전트</param>
        /// <param name="def">act를 포함하는 yaml 정의</param>
        public Act(int index, YamlMappingNode def)
        {
            this.index = index;
            this.def = def;

            LoadOptions();

            LoadScripts();
        }

        /// <summary>
        /// Set Agent to execute with.
        /// </summary>
        /// <param name="agent"></param>
        public void Set(Agent agent)
        {
            this.agent = agent;
            this.globals.agent = agent;
            this.globals.msg = null;
        }

        /// <summary>
        /// Clone to the Agent.
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public Act Clone(Agent agent)
        {
            var nact = new Act();

            nact.Set(agent);

            nact.index          = this.index;
            nact.def            = this.def;
            nact.Name           = this.Name;
            nact.HasBeginScript = this.HasBeginScript;
            nact.HasDoScript = this.HasDoScript;
            nact.HasOnScript = this.HasOnScript;
            nact.HasEndScript = this.HasEndScript;

            return nact;
        }

        /// <summary>
        /// Run Begin Script
        /// </summary>
        /// <returns></returns>
        public Result Begin()
        {
            if ( state == State.Begin)
            {
                return Result.FailContinue;
            }

            state = State.Begin;

            beginTime = DateTime.Now;

            RunScript("begin");

            return Result.SucessContinue;
        }

        /// <summary>
        /// Run End Script
        /// </summary>
        /// <returns></returns>
        public Result End()
        {
            if ( state == State.End)
            {
                return Result.FailContinue;
            }

            state = State.End;

            endTime = DateTime.Now;

            ReportElapsed();

            RunScript("end");

            return Result.SucessContinue;
        }

        /// <summary>
        /// Run Do Script
        /// </summary>
        /// <returns></returns>
        public Result Do()
        {
            Contract.Assert(state == State.Begin);

            RunScript("do");

            return Result.SucessContinue;
        }

        /// <summary>
        /// Run On Script to handle a message
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public Result On(Msg m)
        {
            Contract.Assert(state == State.Begin);

            this.globals.msg = m;

            RunScript("on");

            return Result.SucessContinue;
        }

        private void ReportElapsed()
        {
            Msg m = new Msg();

            m.json["agent"] = agent.get_id();
            m.json["category"] = "act";
            m.json["name"] = Name;
            m.json["begin"] = beginTime.ToString();
            m.json["end"] = endTime.ToString();
            m.json["elapsed"] = (endTime - beginTime).TotalMilliseconds;

            Report.Inst().Notify(m);
        }

        private void RunScript(string func)
        {
            Builder.Inst().Invoke(globals.agent, this, func, globals.msg);
        }

        private void LoadOptions()
        {
            var actNode = (YamlMappingNode)this.def["act"];

            try
            {
                var nameNode = (YamlScalarNode)actNode["name"];
                Name = nameNode.Value;
            }
            catch ( KeyNotFoundException ex)
            {
                Name = "Unknown";
                logger.Error($"Act name not defined");

                throw ex;
            }
        }

        private void LoadScripts()
        {
            var actNode = (YamlMappingNode)this.def["act"];

            try
            {
                var useNode = (YamlScalarNode)actNode["use"];
                var fullName = useNode.Value;

                // use fullName to invoke functions
            }
            catch ( KeyNotFoundException )
            {
                //
            }

            try
            { 
                var doNode = (YamlScalarNode)actNode["do"];
                HasDoScript = Builder.Inst().Load(Name, "do", doNode.Value);
            }
            catch ( KeyNotFoundException )
            {
                logger.Trace($"Act {Name} do not defined");
            }

            try
            {
                var onNode = (YamlScalarNode)actNode["on"];
                HasOnScript = Builder.Inst().Load(Name, "on", onNode.Value);
            }
            catch ( KeyNotFoundException )
            {
                logger.Trace($"Act {Name} on not defined");
            }

            try
            {
                var beginNode = (YamlScalarNode)actNode["begin"];
                HasBeginScript = Builder.Inst().Load(Name, "begin", beginNode.Value);
            }
            catch (KeyNotFoundException )
            {
                logger.Trace($"Begin not defined {Name}");
            }

            try
            {
                var endNode = (YamlScalarNode)actNode["end"];
                HasEndScript = Builder.Inst().Load(Name, "end", endNode.Value);
            }
            catch (KeyNotFoundException )
            {
                logger.Trace($"End not defined {Name}");
            }
        } 
    }
}
