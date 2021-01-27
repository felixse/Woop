using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Woop.Models;
using System.Linq;
using Microsoft.ClearScript.V8;

namespace Woop.Services
{
    public class ScriptManager
    {
        private readonly V8Runtime _runtime;
        private readonly SettingsService _settingsService;

        public ScriptManager(SettingsService settingsService)
        {
            _settingsService = settingsService;

            _runtime = new V8Runtime();
        }

        public async Task<IEnumerable<Script>> InitializeAsync()
        {
            var appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var scriptsFolder = await appInstalledFolder.GetFolderAsync("Assets\\Scripts");

            var requireFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Require.js"));
            var requireScript = await FileIO.ReadTextAsync(requireFile);

            var builtInScripts = await InitializeScripts(scriptsFolder, requireScript, true);

            var customScripts = Enumerable.Empty<Script>();
            if (!string.IsNullOrWhiteSpace(_settingsService.CustomScriptsFolderLocation))
            {
                var folder = await StorageFolder.GetFolderFromPathAsync(_settingsService.CustomScriptsFolderLocation);
                customScripts = await InitializeScripts(folder, requireScript, false);
            }

            return builtInScripts.Concat(customScripts);
        }

        private async Task<IEnumerable<Script>> InitializeScripts(StorageFolder folder, string requireScript, bool builtIn)
        {
            var scripts = new List<Script>();
            foreach (var file in await folder.GetFilesAsync())
            {
                try
                {
                    var script = await InitializeScript(file, requireScript, builtIn);
                    scripts.Add(script);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"{file.Name} failed: {e.Message}");
                    continue;
                }
            }

            return scripts;
        }

        private async Task<Script> InitializeScript(StorageFile file, string requireScript, bool builtIn)
        {
            var content = await FileIO.ReadTextAsync(file);
            return new Script(_runtime, content, requireScript, builtIn);
        }
    }
}
