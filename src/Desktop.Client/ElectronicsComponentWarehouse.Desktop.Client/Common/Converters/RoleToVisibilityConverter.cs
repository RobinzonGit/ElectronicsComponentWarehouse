using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;


namespace ElectronicsComponentWarehouse.Desktop.Client.Common.Converters
{
    /// <summary>
    /// Конвертер роли в видимость (показывает элементы только для администраторов)
    /// </summary>
    public class RoleToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Пример: если роль "Admin", показываем элемент
            if (value is string role && role == "Admin")
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}