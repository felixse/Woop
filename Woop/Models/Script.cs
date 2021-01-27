using Microsoft.ClearScript.V8;
using System;
using System.Text.Json;
using Woop.Services;

namespace Woop.Models
{
    public class Script
    {
        public bool IsBuiltIn { get; }

        public ScriptMetadata Metadata { get; }

        public Lazy<V8ScriptEngine> Context { get; }

        public Script(V8Runtime runtime, string scriptContent, string requireScript, bool builtIn)
        {
            var metaStart = scriptContent.IndexOf("/**");
            var metaEnd = scriptContent.IndexOf("**/");
            var metaContent = scriptContent.Substring(metaStart + 3, metaEnd - metaStart - 3);
            Metadata  = JsonSerializer.Deserialize<ScriptMetadata>(metaContent, new JsonSerializerOptions { AllowTrailingCommas = true, PropertyNameCaseInsensitive = true });
            IsBuiltIn = builtIn;
            Context = new Lazy<V8ScriptEngine>(() => {
                var context = runtime.CreateScriptEngine();
                RequireLoader.EnableRequire(context, requireScript);
                context.Execute(scriptContent);
                return context;
            });
        }

        public string Run(string selection, string fullText, int insertPosition, Action<string> postInfo, Action<string> postError)
        {
            try
            {
                var execution = new ScriptExecution(selection, fullText, insertPosition, postInfo, postError);
                Context.Value.Script.main(execution);
                return execution.text ?? string.Empty;
            }
            catch (Exception e)
            {
                postError(e.Message);
                return selection ?? fullText;
            }
        }
    }
}
