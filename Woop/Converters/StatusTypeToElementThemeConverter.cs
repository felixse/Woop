using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Woop.ViewModels;

namespace Woop.Converters
{
    public class StatusTypeToElementThemeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is StatusViewModel.StatusType type)
            {
                switch (type)
                {
                    case StatusViewModel.StatusType.Normal:
                        return ElementTheme.Default;
                    case StatusViewModel.StatusType.Info:
                    case StatusViewModel.StatusType.Error:
                        return ElementTheme.Dark;
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
