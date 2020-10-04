using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Woop.Models;
using Woop.Services;

namespace Woop.ViewModels
{
    public interface IBuffer
    {
        string GetText();
        void SetText(string text);

        Selection GetSelection();

        void SetSelection(string text);
    }


    public class MainViewModel : ObservableObject
    {
        private const string GetStarted = "Press Ctrl+B to get started";
        private const string SelectYourAction = "Select your action";
        private const string ReloadedScripts = "Reloaded Scripts";

        private readonly ScriptManager _scriptManager;
        private readonly CoreDispatcher _dispatcher;
        private readonly SettingsService _settingsService;
        private string _query;
        private bool _pickerOpened;
        private IEnumerable<ScriptViewModel> _scripts;
        private ObservableCollection<ScriptViewModel> _filteredScripts;
        private ScriptViewModel _selectedScript;
        private ScriptViewModel _lastRunScript;
        private IBuffer _buffer;

        public MainViewModel(CoreDispatcher dispatcher, SettingsService settingsService)
        {
            _dispatcher = dispatcher;
            _settingsService = settingsService;
            _scriptManager = new ScriptManager(settingsService);

            RunSelectedScriptCommand = new RelayCommand(RunSelectedScript);
            ReRunLastScriptCommand = new RelayCommand(ReRunLastScript, () => _lastRunScript != null);
            OpenPickerCommand = new RelayCommand(OpenPicker);
            ClosePickerCommand = new RelayCommand(ClosePicker);
            GetMoreScriptsCommand = new AsyncRelayCommand(GetMoreScripts);
            ClearCommand = new RelayCommand(Clear);
            ReloadScriptsCommand = new AsyncRelayCommand(ReloadScripts);

            FilteredScripts = new ObservableCollection<ScriptViewModel>();

            Status = new StatusViewModel(GetStarted, StatusViewModel.StatusType.Normal);
        }

        public async Task InitializeAsync(IBuffer buffer)
        {
            _buffer = buffer;
            var scripts = await _scriptManager.InitializeAsync();
            _scripts = scripts.Select(s => new ScriptViewModel(s));
        }

        public StatusViewModel Status { get; }

        public IRelayCommand RunSelectedScriptCommand { get; }

        public IRelayCommand ReRunLastScriptCommand { get; }

        public IRelayCommand OpenPickerCommand { get; }

        public IRelayCommand ClosePickerCommand { get; }

        public IAsyncRelayCommand GetMoreScriptsCommand { get; }

        public IRelayCommand ClearCommand { get; }

        public IAsyncRelayCommand ReloadScriptsCommand { get; }

        public ObservableCollection<ScriptViewModel> FilteredScripts
        {
            get => _filteredScripts;
            set => SetProperty(ref _filteredScripts, value);
        }
        
        public ScriptViewModel SelectedScript
        {
            get => _selectedScript;
            set
            {
                if (SelectedScript != null) SelectedScript.IsSelected = false;
                SetProperty(ref _selectedScript, value);
                if (SelectedScript != null) SelectedScript.IsSelected = true;
            }
        }

        public string Query
        {
            get => _query;
            set
            {
                if (SetProperty(ref _query, value))
                {
                    if (value == "*")
                    {
                        FilteredScripts = new ObservableCollection<ScriptViewModel>(_scripts);
                    }
                    else if (string.IsNullOrWhiteSpace(value))
                    {
                        FilteredScripts = new ObservableCollection<ScriptViewModel>(Enumerable.Empty<ScriptViewModel>());
                    }
                    else
                    {
                        FilteredScripts = new ObservableCollection<ScriptViewModel>(_scripts.Where(s => s.Script.Metadata.Name.Contains(value, StringComparison.OrdinalIgnoreCase)));
                    }
                    SelectedScript = FilteredScripts.FirstOrDefault();
                }
            }
        }

        public bool PickerOpened
        {
            get => _pickerOpened;
            private set
            {
                if (SetProperty(ref _pickerOpened, value))
                {
                    Query = null;
                }
            }
        }

        public async Task ReloadScripts()
        {
            ClosePicker();
            _lastRunScript = null;
            await InitializeAsync(_buffer);
            Status.Set(ReloadedScripts, StatusViewModel.StatusType.Success, TimeSpan.FromSeconds(10));
        }

        public void Clear()
        {
            _buffer.SetSelection(null);
            _buffer.SetText(string.Empty);
        }

        public async Task GetMoreScripts()
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/IvanMathy/Boop/tree/main/Scripts"));
        }

        public void OpenPicker()
        {
            Query = null;
            PickerOpened = true;
            Status.Set(SelectYourAction, StatusViewModel.StatusType.Normal);
        }

        public void RunSelectedScript()
        {
            _lastRunScript = SelectedScript;
            ClosePicker();
            var selection = _buffer.GetSelection();
            var text = _lastRunScript.Script.Run(selection.Content, _buffer.GetText(), selection.Start, ShowInfo, ShowError);

            UpdateBuffer(text);
        }

        public void ReRunLastScript()
        {
            var selection = _buffer.GetSelection();
            var text = _lastRunScript.Script.Run(selection.Content, _buffer.GetText(), selection.Start, ShowInfo, ShowError);

            UpdateBuffer(text);
        }

        public void SelectNext()
        {
            var index = FilteredScripts.IndexOf(SelectedScript);
            if (index < FilteredScripts.Count - 1)
            {
                var next = FilteredScripts.ElementAt(index + 1);
                SelectedScript = next;
            }
        }

        public void SelectPrevious()
        {
            var index = FilteredScripts.IndexOf(SelectedScript);
            if (index > 0)
            {
                var next = FilteredScripts.ElementAt(index - 1);
                SelectedScript = next;
            }
        }

        public void ShowError(string error)
        {
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Status.Set(error, StatusViewModel.StatusType.Error, TimeSpan.FromSeconds(10));
            });
        }

        public void ShowInfo(string info)
        {
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Status.Set(info, StatusViewModel.StatusType.Info, TimeSpan.FromSeconds(10));
            });
        }

        public void ClosePicker()
        {
            PickerOpened = false;
            Status.Set(GetStarted, StatusViewModel.StatusType.Normal);
        }

        private void UpdateBuffer(string text)
        {
            if (_buffer.GetSelection()?.Length != 0)
            {
                _buffer.SetSelection(text);
            }
            else
            {
                _buffer.SetText(text);
            }
        }
    }
}
