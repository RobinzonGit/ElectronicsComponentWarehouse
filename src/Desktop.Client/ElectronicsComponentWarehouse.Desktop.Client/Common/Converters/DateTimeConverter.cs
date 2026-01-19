using System;
using System.Globalization;
using System.Windows.Data; 

namespace ElectronicsComponentWarehouse.Desktop.Client.Common.Converters
{
    /// <summary>
    /// Конвертер даты в строку
    /// </summary>
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Поддержка DateTime и Nullable<DateTime>
            if (value is DateTime dateTime)
            {
                return dateTime.ToString("dd.MM.yyyy HH:mm");
            }

            if (value is DateTime)
            {
                var dt = (DateTime)value;
                return dt.ToString("dd.MM.yyyy HH:mm");
            }

            if (value is null)
            {
                return string.Empty;
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}