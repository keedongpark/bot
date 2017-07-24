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
        private Assembly acts;
        private string path;
        private Logger logger = LogManager.GetCurrentClassLogger();

        private Builder()
        {
        }

        /// <summary>
        /// Called when a act function is loaded
        /// </summary>
        /// <param name="actName">Act name</param>
        /// <param name="funcName">Act function name (begin/do/on/end)</param>
        /// <param name="body">Body string of the function</param>
        public bool Load(string actName, string funcName, string body)
        {
            string func_decl = $"public static void {actName}_{funcName}(Agent agent, Msg m) {{\n";
            string func_end = $"\n}} // end func {actName}_{funcName} \n\n";

            code_func += func_decl;
            code_func += body;
            code_func += func_end;

            return true;
        }

        public void Build(string path)
        {
            string cls_ns = "namespace loady {\n";
            string cls_decl = "public class Acts {\n";
            string cls_end = "} // Acts\n";
            string cls_ns_end = "} // loady\n";

            code_all += cls_ns; 
            code_all += cls_decl;
            code_all += code_func;
            code_all += cls_end;
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
                MetadataReference.CreateFromFile(typeof(System.IO.Stream).Assembly.Location), 
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
