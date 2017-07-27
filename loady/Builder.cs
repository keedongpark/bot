using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NLog;

namespace loady
{
    /// <summary>
    /// Acts script function builder
    /// </summary>
    public class Builder
    {
        static Builder builder = new Builder();

        public static Builder Inst()
        {
            return builder;
        }

        private string code_func;
        private string code_all;
        private string current_cls = "Acts";
        private Assembly acts;
        private string path;
        private Logger logger = LogManager.GetCurrentClassLogger();

        private Builder()
        {
        }

        public void Prepare()
        {
            string using_ns = "using System;\n";
            using_ns += "using System.Collections.Generic;\n";
            using_ns += "using loady;\n";
            using_ns += "using Newtonsoft.Json.Linq;\n\n";

            string cls_ns = "namespace loady {\n";

            code_all += using_ns;
            code_all += cls_ns;
        }

        public void Begin(string cls)
        {
            current_cls = cls;
            code_func = "";
        }

        /// <summary>
        /// Called when a act function is loaded
        /// </summary>
        /// <param name="actName">Act name</param>
        /// <param name="funcName">Act function name (begin/do/on/end)</param>
        /// <param name="body">Body string of the function</param>
        public bool Load(string actName, string funcName, string body)
        {
            string func_decl = $"public static void {actName}_{funcName}(Agent agent, Msg msg) {{\n";
            string func_end = $"\n}} // end func {actName}_{funcName} \n\n";

            code_func += func_decl;
            code_func += body;
            code_func += func_end;

            return true;
        }

        public bool LoadFunc(string mod, string func)
        {
            if ( func.Length < 9 ) // to allow empty func 
            {
                return false;
            }

            code_func += "\n";
            code_func += "public static ";
            code_func += func;
            code_func += "\n";

            return true;
        }


        public void End(string cls)
        {
            string cls_decl = $"public class {current_cls} {{\n";
            string cls_end = $"}} // {current_cls}\n";

            code_all += cls_decl;
            code_all += code_func;
            code_all += cls_end;

            code_func = "";
        }

        public void Build(string path)
        {
            string cls_ns_end = "} // loady\n";

            code_all += cls_ns_end;

            this.path = path;

            Save();

            Compile();
        }

        public void Invoke(Agent agent, Act act, string func, Msg msg)
        {
            // call function from compiled assembly

            Type type = acts.GetType("loady.Acts");

            var fn = $"{act.Name}_{func}";

            MethodInfo methodInfo = type.GetMethod(fn);

            if (methodInfo != null)
            {
                var args = new object[]
                {
                    agent, 
                    msg
                };

                methodInfo.Invoke(this, args);
            }
        }

        private void Save()
        {
            var dir = Path.GetDirectoryName(path);
            FileStream fs = File.Open(dir + "\\loady.acts.cs", FileMode.Create);

            var sw = new StreamWriter(fs);

            sw.Write(code_all);

            sw.Close();
        }

        private void Compile()
        {
            IEnumerable<string> DefaultNamespaces =
            new[]
            {
                "System",
                "System.IO",
                "Newtonsoft.Json.Linq", 
                "loady"
            };

            IEnumerable<MetadataReference> DefaultReferences =
                new[]
                {
                MetadataReference.CreateFromFile(typeof(System.Object).Assembly.Location), 
                MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<int>).Assembly.Location), 
                MetadataReference.CreateFromFile(typeof(System.IO.Stream).Assembly.Location), 
                MetadataReference.CreateFromFile(typeof(System.Dynamic.IDynamicMetaObjectProvider).Assembly.Location), 
                MetadataReference.CreateFromFile(typeof(Newtonsoft.Json.Linq.JObject).Assembly.Location), 
                MetadataReference.CreateFromFile(typeof(loady.Act).Assembly.Location), 
                };

            CSharpCompilationOptions DefaultCompilationOptions =
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                        .WithOverflowChecks(true).WithOptimizationLevel(OptimizationLevel.Release)
                        .WithUsings(DefaultNamespaces);

            var tree = CSharpSyntaxTree.ParseText(code_all);
            var compilation = CSharpCompilation.Create("loady.acts.dll", new SyntaxTree[] { tree }, DefaultReferences, DefaultCompilationOptions);

            var stream = new MemoryStream();
            var emitResult = compilation.Emit(stream);

            if (emitResult.Success)
            {
                stream.Seek(0, SeekOrigin.Begin);
                acts = Assembly.Load(stream.GetBuffer());
            }
            else
            {
                foreach (var err in emitResult.Diagnostics)
                {
                    logger.Error(err);
                }
            }
        }
    }
}
