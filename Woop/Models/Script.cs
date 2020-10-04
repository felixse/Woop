using ChakraCore.NET;
using System;
using System.Text.Json;
using Woop.Services;

namespace Woop.Models
{
    public class Script
    {
        public bool IsBuiltIn { get; }

        public ScriptMetadata Metadata { get; }

        public Lazy<ChakraContext> Context { get; }

        public Script(ChakraRuntime runtime, string scriptContent, string adapterScript, string requireScript, bool builtIn)
        {
            var metaStart = scriptContent.IndexOf("/**");
            var metaEnd = scriptContent.IndexOf("**/");
            var metaContent = scriptContent.Substring(metaStart + 3, metaEnd - metaStart - 3);
            Metadata  = JsonSerializer.Deserialize<ScriptMetadata>(metaContent, new JsonSerializerOptions { AllowTrailingCommas = true, PropertyNameCaseInsensitive = true });
            IsBuiltIn = builtIn;
            Context = new Lazy<ChakraContext>(() => {
                var context = runtime.CreateContext(true);
                RequireLoader.EnableRequire(context, requireScript);
                context.RunScript(scriptContent);
                context.RunScript(adapterScript);
                return context;
            });
        }

        public string Run(string selection, string fullText, int insertPosition, Action<string> postInfo, Action<string> postError)
        {
            var input = new ScriptExecutionProperties(selection, fullText, insertPosition);
            var methods = new ScriptExecutionMethods(postInfo, postError);
            try
            {
                return Context.Value.GlobalObject.CallFunction<ScriptExecutionProperties, ScriptExecutionMethods, string>("woopAdapter", input, methods);
            }
            catch (Exception e)
            {
                postError(e.Message);
                return selection ?? fullText;
            }
        }
    }
}
