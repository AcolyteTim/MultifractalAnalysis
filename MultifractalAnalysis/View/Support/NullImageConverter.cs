using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace MultifractalAnalysis.View.Support
{
    /// <summary>
    /// Конвертер для преобразования <see cref="null"/> в источник изображения (???).
    /// </summary>
    public class NullImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ?? DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
