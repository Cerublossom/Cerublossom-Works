using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace service.ViewModels
{
    /// <summary>
    /// Конвертер фона слота календаря в зависимости от занятости.
    /// </summary>
    public class SlotBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isBooked = value is bool && (bool)value;
            // Занято — зеленый (как в макете), свободно — светло-серый
            return isBooked ? new SolidColorBrush(Color.FromRgb(0x4C, 0x7A, 0x6A))
                            : new SolidColorBrush(Color.FromRgb(0xE8, 0xEF, 0xEB));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
