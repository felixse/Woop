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
using Woop.Models;

namespace Woop.Views
{
    public sealed partial class MainPage : Page
    {
        private CoreApplicationViewTitleBar _coreTitleBar = CoreApplication.GetCurrentView().TitleBar;

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
            ViewModel = new MainViewModel(Dispatcher);

            InitializeComponent();

            Window.Current.SetTitleBar(TitleBar);

            Selector.Translation += new Vector3(0, 0, 32);

            ApplicationView.PreferredLaunchViewSize = new Size(480, 480);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;

            titleBar.BackgroundColor = Windows.UI.Colors.Transparent;
            titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
            titleBar.InactiveBackgroundColor = Windows.UI.Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.InitializeAsync();
            Buffer.Focus(FocusState.Programmatic);
        }

        private void Query_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Escape)
            {
                ViewModel.ClosePicker();
                Buffer.Focus(FocusState.Programmatic);
            } 
            else if (e.Key == VirtualKey.Enter)
            {
                ViewModel.RunSelectedScript();
                Buffer.Focus(FocusState.Programmatic);
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
            ViewModel = null;
            _coreTitleBar = null;
        }

        private void Buffer_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Buffer.SelectedText))
            {
                ViewModel.Selection = new Selection
                {
                    Start = Buffer.SelectionStart,
                    Length = Buffer.SelectionLength,
                    Content = Buffer.SelectedText
                };
            }
            else
            {
                ViewModel.Selection = null;
            }
        }
    }
}
