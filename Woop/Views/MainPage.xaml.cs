using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.Core;
using Windows.System;
using System.Numerics;
using Woop.ViewModels;
using Woop.Services;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls.Primitives;

namespace Woop.Views
{
    public sealed partial class MainPage : Page
    {
        private CoreApplicationViewTitleBar _coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
        private readonly SettingsService _settingsService;
        private readonly long _isOpenPropertyChangedCallbackToken;

        public double CoreTitleBarHeight => _coreTitleBar.Height;

        public Thickness CoreTitleBarPadding
        {
            get
            {
                if (FlowDirection == FlowDirection.LeftToRight)
                {
                    return new Thickness { Left = _coreTitleBar.SystemOverlayLeftInset, Right = _coreTitleBar.SystemOverlayRightInset };
                }
                else
                {
                    return new Thickness { Left = _coreTitleBar.SystemOverlayRightInset, Right = _coreTitleBar.SystemOverlayLeftInset };
                }
            }
        }

        public MainViewModel ViewModel { get; private set; }

        public MainPage()
        {
            _settingsService = new SettingsService();
            _settingsService.ApplicationThemeChanged += OnApplicationThemeChanged;
            ViewModel = new MainViewModel(Dispatcher, _settingsService);

            RequestedTheme = _settingsService.ApplicationTheme;

            InitializeComponent();

            Window.Current.SetTitleBar(TitleBar);

            Selector.Translation += new Vector3(0, 0, 32);

            ApplicationView.PreferredLaunchViewSize = new Size(480, 480);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            SetTitleBarColors();

            _isOpenPropertyChangedCallbackToken = SelectorPopup.RegisterPropertyChangedCallback(Popup.IsOpenProperty, (s, e) =>
            {
                if (SelectorPopup.IsOpen)
                {
                    FocusAction.TargetObject = Query;
                    FocusAction.Execute(s, e);
                }
            });
        }



        private void SetTitleBarColors()
        {
            // todo resources color are not correct when theme changes. hardcode values for each theme?

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;

            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            titleBar.ButtonHoverBackgroundColor = (App.Current.Resources["TitleBarButtonPointerOverBackground"] as SolidColorBrush).Color;
            titleBar.ButtonPressedBackgroundColor = (App.Current.Resources["TitleBarButtonPressedBackground"] as SolidColorBrush).Color;

            titleBar.ButtonForegroundColor = (Color)Resources["SystemBaseHighColor"];
            titleBar.ButtonHoverForegroundColor = (Color)Resources["SystemBaseHighColor"];
            titleBar.ButtonPressedForegroundColor = (Color)Resources["SystemBaseHighColor"];
        }

        private async void OnApplicationThemeChanged(object sender, ElementTheme e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                RequestedTheme = e;
                SetTitleBarColors();
            });
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LineNumbers.Initialize(Buffer);
            await ViewModel.InitializeAsync(Buffer);
        }

        private void Query_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                ViewModel.RunSelectedScript();
            }
            else if (e.Key == VirtualKey.Up)
            {
                ViewModel.SelectPrevious();
            }
            else if (e.Key == VirtualKey.Down)
            {
                ViewModel.SelectNext();
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Scripts.ScrollIntoView(ViewModel.SelectedScript);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SetTitleBar(null);
            _settingsService.ApplicationThemeChanged -= OnApplicationThemeChanged;
            SelectorPopup.UnregisterPropertyChangedCallback(Popup.IsOpenProperty, _isOpenPropertyChangedCallbackToken);
            ViewModel = null;
            _coreTitleBar = null;
        }

        private async void OnSettingsTapped(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsDialog(_settingsService);
            await settings.ShowAsync();
        }
    }
}
