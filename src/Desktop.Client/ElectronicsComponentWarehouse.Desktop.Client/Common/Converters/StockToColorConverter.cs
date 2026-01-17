using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ElectronicsComponentWarehouse.Desktop.Client.Common.Converters
{
    /// <summary>
    /// Конвертер количества в цвет (для индикации низкого запаса)
    /// </summary>
    public class StockToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stockQuantity)
            {
                // Для простоты используем фиксированные уровни
                if (stockQuantity == 0)
                {
                    return Brushes.Red; // Нет в наличии
                }
                else if (stockQuantity <= 10) // 10 - минимальный уровень по умолчанию
                {
                    return Brushes.Orange; // Низкий запас
                }
                else
                {
                    return Brushes.Green; // Достаточно
                }
            }

            return Brushes.Gray; // По умолчанию
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}