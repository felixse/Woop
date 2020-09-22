using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Woop.Converters
{
    public class ElementThemeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ElementTheme elementTheme)
            {
                switch (elementTheme)
                {
                    case ElementTheme.Default:
                        return "System default";
                    case ElementTheme.Light:
                        return "Light";
                    case ElementTheme.Dark:
                        return "Dark";
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
