using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Woop.Services;
using Woop.ViewModels;

namespace Woop.Views
{
    public sealed partial class SettingsDialog : ContentDialog
    {
        private readonly SettingsService _settingsService;

        public SettingsDialog(SettingsService settingsService)
        {
            _settingsService = settingsService;
            _settingsService.ApplicationThemeChanged += OnApplicationThemeChanged;
            ViewModel = new SettingsViewModel(settingsService);
            RequestedTheme = ViewModel.SelectedApplicationTheme;

            InitializeComponent();
        }

        public SettingsViewModel ViewModel { get; }

        private void OnCloseTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Hide();
        }

        private async void OnApplicationThemeChanged(object sender, ElementTheme e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => RequestedTheme = e);
        }

        private void ContentDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            _settingsService.ApplicationThemeChanged -= OnApplicationThemeChanged;
        }
    }
}
