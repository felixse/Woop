using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace Woop.Converters
{
    public class IconNameToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var theme = Application.Current.RequestedTheme == ApplicationTheme.Light ? "light" : "dark";

            if (value is string iconName && !string.IsNullOrWhiteSpace(iconName))
            {

                return new BitmapImage(new Uri($"ms-appx:///Assets/{theme}/{iconName}.png"));
            }

            return new BitmapImage(new Uri($"ms-appx:///Assets/{theme}/unknown.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
