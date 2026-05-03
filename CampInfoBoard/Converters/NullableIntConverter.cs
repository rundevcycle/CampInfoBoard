using System.Globalization;
using System.Windows.Data;

namespace CampInfoBoard.Converters
{
    public class NullableIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? "";
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? text = value as string;

            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            if (int.TryParse(text, out int result))
            {
                return result;
            }

            return WpfBinding.DoNothing;
        }
    }
}