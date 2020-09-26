using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using Windows.UI.Xaml;

namespace Woop.ViewModels
{
    public class StatusViewModel : ObservableObject
    {
        public enum StatusType
        {
            Normal,
            Info,
            Success,
            Error
        }

        private readonly DispatcherTimer _timer;

        private string _text;
        private StatusType _type;
        private string _defaultText;
        private StatusType _defaultType;

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public StatusType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        
        public StatusViewModel(string text, StatusType type)
        {
            Text = text;
            Type = type;

            _defaultText = text;
            _defaultType = type;

            _timer = new DispatcherTimer();
            _timer.Tick += OnTimerElapsed;
        }

        public void Set(string text, StatusType type, TimeSpan? duration = null)
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
            }

            Text = text;
            Type = type;

            if (duration.HasValue)
            {
                _timer.Interval = duration.Value;
                _timer.Start();
            }
        }

        private void OnTimerElapsed(object sender, object e)
        {
            Text = _defaultText;
            Type = _defaultType;
        }
    }
}
