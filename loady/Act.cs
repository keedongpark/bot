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
        private Script<object> beginScript = null;
        private Script<object> endScript = null;
        private Script<object> doScript = null;
        private Script<object> onScript = null;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private State state = State.None;

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
        /// Has Begun  
        /// </summary>
        public bool IsInBeginState { get { return state == State.Begin; } }

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

            nact.Set(agent);

            nact.index          = this.index;
            nact.def            = this.def;
            nact.beginScript    = this.beginScript;
            nact.endScript      = this.endScript;
            nact.doScript       = this.doScript;
            nact.onScript       = this.onScript;

            return nact;
        }

        /// <summary>
        /// Sometimes we need to change index
        /// </summary>
        /// <param name="index"></param>
        public void Reindex(int index)
        {
            this.index = index;
        }

        public Result Begin()
        {
            Contract.Assert(state != State.Begin);

            state = State.Begin;   

            if (beginScript == null)
            {
                return Result.FailContinue;
            }

            try
            {
                beginScript.RunAsync(globals);
            }
            catch (CompilationErrorException ex)
            {
                logger.Error($"Act {Name} Exception: {ex}");
                logger.Error($"Code> {beginScript.Code}");

                return Result.FailStopException;
            }

            return Result.SucessContinue;
        }

        public Result End()
        {
            Contract.Assert(state != State.End);

            state = State.End;

            if (endScript == null)
            {
                return Result.FailContinue;
            }

            try
            {
                endScript.RunAsync(globals);
            }
            catch (CompilationErrorException ex)
            {
                logger.Error($"Act {Name} Exception: {ex}");
                logger.Error($"Code> {endScript.Code}");

                return Result.FailStopException;
            }

            return Result.SucessContinue;
        }

        public Result Do()
        {
            Contract.Assert(state == State.Begin);

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
                logger.Error($"Act {Name} Exception: {ex}");
                logger.Error($"Code> {doScript.Code}");

                return Result.FailStopException;
            }

            return Result.SucessContinue;
        }

        public Result On(Msg m)
        {
            Contract.Assert(state == State.Begin);

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
                logger.Error($"Act {Name} Exception: {ex}");
                logger.Error($"Code> {onScript.Code}");

                return Result.FailStopException;
            }

            return Result.SucessContinue;
        }

        private void LoadOptions()
        {
            var actNode = (YamlMappingNode)this.def["act"];

            try
            {
                var nameNode = (YamlScalarNode)actNode["name"];
                Name = nameNode.Value;
            }
            catch ( KeyNotFoundException )
            {
                Name = "Unknown";
                logger.Error($"Act name not defined");
            }
        }

        private void BuildScripts()
        {
            var actNode = (YamlMappingNode)this.def["act"];

            try
            {
                var useNode = (YamlScalarNode)actNode["use"];
                var fullName = useNode.Value;
                var act = Mods.Inst().Get(fullName);

                if ( act != null )
                {
                    CopyScriptsFrom(act);
                }
                else
                {
                    throw new InvalidOperationException($"use must have a valid act in module {fullName}. Current {Name}");
                }
            }
            catch ( KeyNotFoundException )
            {
                //
            }

            try
            {
                var doNode = (YamlScalarNode)actNode["do"];
                doScript = CSharpScript.Create(doNode.Value, globalsType: typeof(Globals));
                doScript.Compile();
            }
            catch ( KeyNotFoundException )
            {
                logger.Trace($"Act {Name} do not defined");
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
            catch ( KeyNotFoundException )
            {
                logger.Trace($"Act {Name} on not defined");
            }
            catch ( CompilationErrorException ex)
            {
                logger.Error(ex);
                throw;
            }

            try
            {
                var enterNode = (YamlScalarNode)actNode["begin"];
                beginScript = CSharpScript.Create(enterNode.Value, globalsType: typeof(Globals));
                beginScript.Compile();
            }
            catch (KeyNotFoundException )
            {
                logger.Trace($"Begin not defined {Name}");
            }
            catch (CompilationErrorException ex)
            {
                logger.Error(ex);
                throw;
            }

            try
            {
                var exitNode = (YamlScalarNode)actNode["end"];
                endScript = CSharpScript.Create(exitNode.Value, globalsType: typeof(Globals));
                endScript.Compile();
            }
            catch (KeyNotFoundException )
            {
                logger.Trace($"End not defined {Name}");
            }
            catch (CompilationErrorException ex)
            {
                logger.Error(ex);
                throw;
            }
        } 

        private void CopyScriptsFrom(Act other)
        {
            beginScript    = other.beginScript;
            endScript      = other.endScript;
            doScript       = other.doScript;
            onScript       = other.onScript;
        }
    }
}
