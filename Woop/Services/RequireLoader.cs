//using ChakraCore.NET;
using Microsoft.ClearScript.V8;
using System.Collections.Generic;
using System.IO;

namespace Woop.Services
{
    public class RequireLoader
    {
        private readonly Dictionary<string, string> _scriptCache = new Dictionary<string, string>();

        public string RootPath { get; set; } = string.Empty;

        public string LoadLib(string name)
        {
            if (!_scriptCache.ContainsKey(name))
            {
                _scriptCache.Add(name, Load(name));
            }
            return _scriptCache[name];
        }

        public static void EnableRequire(V8ScriptEngine engine, string requireScript, string rootPath = null)
        {
            RequireLoader loader = new RequireLoader() { RootPath = rootPath };
            engine.AddHostObject("_loader", loader);
            engine.Execute(requireScript);
        }

        public string Load(string name)
        {
            name = name.Replace("@boop/", $"Assets/Scripts/lib/");
            var directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\" + RootPath);

            var fileName = name + ".js";
            var files = directoryInfo.GetFiles(fileName);
            if (files.Length == 1)
            {
                return files[0].OpenText().ReadToEnd();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
