using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace service.ViewModels
{
    // Превращает путь к картинке в BitmapImage.
    // Поддерживает локальные файлы (Images/...) и ссылки http(s).
    // Если картинку загрузить не удалось — возвращает null,
    // и в карточке ресурса остаётся заглушка "ФОТО".
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;
            if (string.IsNullOrWhiteSpace(path))
                return null;

            try
            {
                // Ссылка на фото в интернете
                if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    var remote = new BitmapImage();
                    remote.BeginInit();
                    remote.UriSource = new Uri(path, UriKind.Absolute);
                    remote.EndInit();
                    return remote;
                }

                // Обычный файл рядом с программой
                if (!Path.IsPathRooted(path))
                    path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

                if (!File.Exists(path))
                    return null;

                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // чтобы файл не был занят
                image.UriSource = new Uri(path, UriKind.Absolute);
                image.EndInit();
                return image;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
