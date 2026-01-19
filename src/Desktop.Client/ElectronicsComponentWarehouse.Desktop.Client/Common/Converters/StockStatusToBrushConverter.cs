using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ElectronicsComponentWarehouse.Desktop.Client.Common.Converters
{
    /// <summary>
    /// Конвертер статуса запаса в кисть для отображения цвета
    /// </summary>
    public class StockStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "Низкий запас" => new SolidColorBrush(Color.FromRgb(255, 152, 0)), // Orange
                    "Нет в наличии" => new SolidColorBrush(Color.FromRgb(244, 67, 54)), // Red
                    "Достаточно" => new SolidColorBrush(Color.FromRgb(76, 175, 80)), // Green
                    _ => new SolidColorBrush(Color.FromRgb(158, 158, 158)) // Gray
                };
            }

            return new SolidColorBrush(Color.FromRgb(158, 158, 158));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}