using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using ChakraCore.NET;
using Woop.Models;

namespace Woop.Services
{
    class ScriptManager
    {
        private readonly ChakraRuntime _runtime;

        public ScriptManager()
        {
            _runtime = ChakraRuntime.Create();
            _runtime.ServiceNode.GetService<IJSValueConverterService>().RegisterStructConverter((value, instance) =>
            {
                value.WriteProperty("fullText", instance.FullText);
                value.WriteProperty("selection", instance.Selection ?? string.Empty);
                value.WriteProperty("isSelection", instance.IsSelection);
            }, (value) =>
            {
                var fullText = value.ReadProperty<string>("fullText");
                var selection = value.ReadProperty<string>("selection");
                return new ScriptExecutionProperties(selection, fullText);
            });

            _runtime.ServiceNode.GetService<IJSValueConverterService>().RegisterProxyConverter<ScriptExecutionMethods>((binding, instance, serviceNode) =>
            {
                binding.SetMethod<string>("postInfo", instance.PostInfo);
                binding.SetMethod<string>("postError", instance.PostError);
                binding.SetMethod<string>("insert", instance.Insert);
            });
        }

        public async Task<IEnumerable<Script>> InitializeAsync()
        {
            var appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var scriptsFolder = await appInstalledFolder.GetFolderAsync("Assets\\Scripts");


            var scripts = new List<Script>();
            var adapterFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/WoopAdapter.js"));
            var adapterScript = await ReadScriptContentAsync(adapterFile);

            foreach (var file in await scriptsFolder.GetFilesAsync())
            {
                try
                {
                    var content = await ReadScriptContentAsync(file);
                    var context = _runtime.CreateContext(true);
#pragma warning disable CS0618 // Type or member is obsolete
                    JSRequireLoader.EnableRequire(context);
#pragma warning restore CS0618 // Type or member is obsolete



                    var result = context.RunScript(content);
                    result = context.RunScript(adapterScript);
                    scripts.Add(new Script(context, content));
                }
                catch (Exception e)
                {

                    Debug.WriteLine($"{file.Name} failed: {e.Message}");
                    continue;
                }
            }

            return scripts;
        }

        private async Task<string> ReadScriptContentAsync(StorageFile file)
        {
            var script = await FileIO.ReadTextAsync(file);
            script = script.Replace("require('@boop/", $"require('Assets/Scripts/lib/");
            return script;
        }
    }
}
