using ChakraCore.NET;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Woop.Models
{
    public class Script
    {
        public bool IsBuiltIn { get; }

        public ScriptMetadata Metadata { get; }

        public ChakraContext Context { get; }

        public Script(ChakraContext context, string scriptContent, bool builtIn)
        {
            var metaStart = scriptContent.IndexOf("/**");
            var metaEnd = scriptContent.IndexOf("**/");
            var metaContent = scriptContent.Substring(metaStart + 3, metaEnd - metaStart - 3);
            Metadata  = JsonSerializer.Deserialize<ScriptMetadata>(metaContent, new JsonSerializerOptions { AllowTrailingCommas = true, PropertyNameCaseInsensitive = true });
            IsBuiltIn = builtIn;
            Context = context;
        }

        public string Run(string selection, string fullText, Action<string> postInfo, Action<string> postError, Action<string> insert)
        {
            var input = new ScriptExecutionProperties(selection, fullText);
            var methods = new ScriptExecutionMethods(postInfo, postError, insert);
            try
            {
                return Context.GlobalObject.CallFunction<ScriptExecutionProperties, ScriptExecutionMethods, string>("woopAdapter", input, methods);
            }
            catch (Exception e)
            {
                postError(e.Message);
                return selection ?? fullText;
            }
        }
    }
}
