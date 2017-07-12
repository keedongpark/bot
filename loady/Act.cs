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

        // 기본 필드들
        private int index = -1;
        private YamlMappingNode def = null;
        private Agent agent = null;

        // 스크립트 관련 필드
        private Globals globals = new Globals();
        private Script<object> doScript = null;
        private Script<object> onScript = null;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// get Agent
        /// </summary>
        public Agent Agent { get { return agent; } }

        /// <summary>
        /// get index in a flow 
        /// </summary>
        public int Index {  get { return index; } }

        /// <summary>
        /// 실행 스크립트를 가짐
        /// </summary>
        public bool HasDoScript {  get { return doScript != null; } }

        /// <summary>
        /// msg 처리 스크립트를 가짐 
        /// </summary>
        public bool HasOnScript {  get { return onScript != null; } }

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

            BuildScripts();
        }

        public void Set(Agent agent)
        {
            this.agent = agent;
            this.globals.agent = agent;
            this.globals.msg = null;
        }

        public Act Clone(Agent agent)
        {
            var nact = new Act();

            nact.index = this.index;
            nact.def = this.def;
            nact.Set(agent);
            nact.doScript = this.doScript;
            nact.onScript = this.onScript;

            return nact;
        }

        public Result Do()
        {
            if ( doScript == null)
            {
                return Result.FailContinue;
            }

            try
            {
                doScript.RunAsync(globals);
            }
            catch ( CompilationErrorException ex)
            {
                logger.Error(ex);

                return Result.FailStopException;
            }

            return Result.SucessContinue;
        }

        public Result On(Msg m)
        {
            if ( onScript == null)
            {
                return Result.FailContinue;
            }

            this.globals.msg = m;

            try
            {
                onScript.RunAsync(globals);
            }
            catch (CompilationErrorException ex)
            {
                logger.Error(ex);

                return Result.FailStopException;
            }

            return Result.SucessContinue;
        }

        private void BuildScripts()
        {
            var actNode = (YamlMappingNode)this.def["act"];

            try
            {
                var doNode = (YamlScalarNode)actNode["do"];
                doScript = CSharpScript.Create(doNode.Value, globalsType: typeof(Globals));
                doScript.Compile();
            }
            catch ( KeyNotFoundException ex)
            {
                logger.Error(ex);
            }
            catch ( CompilationErrorException ex)
            {
                logger.Error(ex);
                throw;
            }

            try
            {
                var onNode = (YamlScalarNode)actNode["on"];
                onScript = CSharpScript.Create(onNode.Value, globalsType: typeof(Globals));
                onScript.Compile();
            }
            catch ( KeyNotFoundException ex)
            {
                logger.Error(ex);
            }
            catch ( CompilationErrorException ex)
            {
                logger.Error(ex);
                throw;
            }
        } 
    }
}
