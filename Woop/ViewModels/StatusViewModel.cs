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
            Error
        }

        private readonly DispatcherTimer _timer;

        private string _text;
        private StatusType _type;
        private string _nextText;
        private StatusType _nextType;

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

            _timer = new DispatcherTimer();
            _timer.Tick += OnTimerElapsed;
        }

        public void Set(string text, StatusType type)
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
            }

            Text = text;
            Type = type;

        }

        public void Set(string text, StatusType type, TimeSpan duration, string nextText, StatusType nextType)
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
            }

            _nextText = nextText;
            _nextType = nextType;

            Text = text;
            Type = type;

            _timer.Interval = duration;
            _timer.Start();
        }

        private void OnTimerElapsed(object sender, object e)
        {
            Text = _nextText;
            Type = _nextType;
        }
    }
}
