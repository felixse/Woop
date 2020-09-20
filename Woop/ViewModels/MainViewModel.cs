using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Woop.Models;
using Woop.Services;

namespace Woop.ViewModels
{

    public class MainViewModel : ObservableObject
    {
        private const string GetStarted = "Press Ctrl+B to get started";
        private const string SelectYourAction = "Select your action";

        private readonly ScriptManager _scriptManager;
        private readonly CoreDispatcher _dispatcher;
        private string _buffer;
        private Selection _selection;
        private string _query;
        private bool _pickerOpened;
        private IEnumerable<ScriptViewModel> _scripts;
        private ObservableCollection<ScriptViewModel> _filteredScripts;
        private ScriptViewModel _selectedScript;
        private ScriptViewModel _lastRunScript;

        public MainViewModel(CoreDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _scriptManager = new ScriptManager();

            RunSelectedScriptCommand = new RelayCommand(RunSelectedScript);
            ReRunLastScriptCommand = new RelayCommand(ReRunLastScript, () => _lastRunScript != null);
            OpenPickerCommand = new RelayCommand(OpenPicker);

            FilteredScripts = new ObservableCollection<ScriptViewModel>();

            Status = new StatusViewModel(GetStarted, StatusViewModel.StatusType.Normal);
        }

        public async Task InitializeAsync()
        {
            var scripts = await _scriptManager.InitializeAsync();

            _scripts = scripts.Select(s => new ScriptViewModel(s));
        }

        public StatusViewModel Status { get; }

        public IRelayCommand RunSelectedScriptCommand { get; }

        public IRelayCommand ReRunLastScriptCommand { get; }

        public IRelayCommand OpenPickerCommand { get; }

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

        public string Buffer
        {
            get => _buffer;
            set => SetProperty(ref _buffer, value);

        }

        public Selection Selection
        {
            get => _selection;
            set => SetProperty(ref _selection, value);

        }

        public bool PickerOpened
        {
            get => _pickerOpened;
            set
            {
                if (SetProperty(ref _pickerOpened, value))
                {
                    if (PickerOpened)
                    {
                        Status.Set(SelectYourAction, StatusViewModel.StatusType.Normal);
                    }
                    Query = null;
                }
            }
        }

        public void OpenPicker()
        {
            PickerOpened = true;
        }

        public void RunSelectedScript()
        {
            _lastRunScript = SelectedScript;
            var text = SelectedScript.Script.Run(Selection?.Content, Buffer, ShowInfo, ShowError, _ => { });
            UpdateBuffer(text);
        }

        public void ReRunLastScript()
        {
            var text = _lastRunScript.Script.Run(Selection?.Content, Buffer, ShowInfo, ShowError, _ => { });
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
                Status.Set(error, StatusViewModel.StatusType.Error, TimeSpan.FromSeconds(10), GetStarted, StatusViewModel.StatusType.Normal);
            });
        }

        public void ShowInfo(string info)
        {
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Status.Set(info, StatusViewModel.StatusType.Info, TimeSpan.FromSeconds(10), GetStarted, StatusViewModel.StatusType.Normal);
            });
        }

        private void UpdateBuffer(string text)
        {
            if (Selection != null)
            {
                Buffer = Buffer.Remove(Selection.Start, Selection.Length);
                Buffer = Buffer.Insert(Selection.Start, text);
            }
            else
            {
                Buffer = text;
            }
        }
    }
}
