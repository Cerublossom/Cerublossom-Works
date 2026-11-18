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
            // Серые тона: свободно — светло-серый, занято — темно-серый
            return isBooked ? new SolidColorBrush(Color.FromRgb(0x9C, 0x27, 0x27))
                            : new SolidColorBrush(Color.FromRgb(0x4A, 0x7C, 0x5F));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
