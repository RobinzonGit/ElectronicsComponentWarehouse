// InverseBooleanConverter.cs в папке Common/Converters
using System;
using System.Globalization;
using System.Windows.Data;

namespace ElectronicsComponentWarehouse.Desktop.Client.Common.Converters
{
    /// <summary>
    /// Инвертирует булево значение
    /// </summary>
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool boolValue ? !boolValue : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool boolValue ? !boolValue : false;
        }
    }
}