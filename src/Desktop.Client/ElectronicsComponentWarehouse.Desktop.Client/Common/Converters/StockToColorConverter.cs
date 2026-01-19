using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ElectronicsComponentWarehouse.Desktop.Client.Common.Converters
{
    /// <summary>
    /// Конвертер количества в цвет (для индикации низкого запаса)
    /// </summary>
    public class StockToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 &&
                values[0] is int stockQuantity &&
                values[1] is int minimumStockLevel)
            {
                if (stockQuantity == 0)
                {
                    return Brushes.Red; // Нет в наличии
                }
                else if (stockQuantity <= minimumStockLevel)
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}