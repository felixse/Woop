using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Woop.Services;

namespace Woop.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly SettingsService _settingsService;
        private bool _invalidDirectory;

        public SettingsViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;

            ElementThemes = Enum.GetValues(typeof(ElementTheme)).Cast<ElementTheme>();

            BrowseCommand = new AsyncRelayCommand(Browse);
        }

        public IAsyncRelayCommand BrowseCommand { get; }

        public IEnumerable<ElementTheme> ElementThemes { get; }

        public ElementTheme SelectedApplicationTheme
        {
            get => _settingsService.ApplicationTheme;
            set
            {
                if (_settingsService.ApplicationTheme != value)
                {
                    _settingsService.ApplicationTheme = value;
                    OnPropertyChanged(nameof(SelectedApplicationTheme));
                }
            }
        }

        public string CustomScriptsFolderLocation
        {
            get => _settingsService.CustomScriptsFolderLocation;
            set
            {
                var valid = Directory.Exists(value);
                InvalidDirectory = !valid;
                if (valid)
                {
                    _settingsService.CustomScriptsFolderLocation = value;
                    OnPropertyChanged(nameof(CustomScriptsFolderLocation));
                    UpdateFutureAccessList(value);
                }
            }
        }

        public bool InvalidDirectory
        {
            get => _invalidDirectory;
            set => SetProperty(ref _invalidDirectory, value);
        }

        public async Task Browse()
        {
            var picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add("*");

            var folder = await picker.PickSingleFolderAsync();

            if (folder != null)
            {
                CustomScriptsFolderLocation = folder.Path;
            }
        }

        private async Task UpdateFutureAccessList(string path)
        {
            var file = await StorageFile.GetFileFromPathAsync(CustomScriptsFolderLocation);
            StorageApplicationPermissions.FutureAccessList.AddOrReplace("CustomScriptsToken", file);
        }
    }
}
