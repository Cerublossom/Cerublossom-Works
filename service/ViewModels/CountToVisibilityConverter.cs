using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace service.ViewModels
{
    // Показывает элемент, если количество = 0 (например, надпись "Нет бронирований")
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count = 0;
            if (value is int)
                count = (int)value;
            else if (value is System.Collections.ICollection)
                count = ((System.Collections.ICollection)value).Count;
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
