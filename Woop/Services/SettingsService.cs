using System;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Woop.Services
{
    public class SettingsService : IDisposable
    {
        private const string ApplicationThemeKey = "ApplicationTheme";
        private const string CustomScriptsFolderLocationKey = "CustomScriptsFolderLocation";

        private readonly UISettings _uISettings;
        private bool disposedValue;

        public event EventHandler<ElementTheme> ApplicationThemeChanged;

        public SettingsService()
        {
            _uISettings = new UISettings();
            _uISettings.ColorValuesChanged += OnColorValuesChanged;
        }

        public ElementTheme ApplicationTheme
        {
            get => Enum.Parse<ElementTheme>(ApplicationData.Current.LocalSettings.Values[ApplicationThemeKey] as string ?? nameof(ElementTheme.Default), true);
            set
            {
                ApplicationData.Current.LocalSettings.Values[ApplicationThemeKey] = value.ToString();
                ApplicationThemeChanged(this, value);
            }
        }

        public string CustomScriptsFolderLocation
        {
            get => ApplicationData.Current.LocalSettings.Values[CustomScriptsFolderLocationKey] as string;
            set => ApplicationData.Current.LocalSettings.Values[CustomScriptsFolderLocationKey] = value;
        }

        private void OnColorValuesChanged(UISettings sender, object args)
        {
            ApplicationThemeChanged?.Invoke(this, ApplicationTheme);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _uISettings.ColorValuesChanged -= OnColorValuesChanged;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
