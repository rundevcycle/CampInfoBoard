using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CampInfoBoard.Converters
{
    public class ScaleValueConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            double scale = 1.0;

            if (value is double doubleValue)
            {
                scale = doubleValue;
            }

            if (parameter == null ||
                !double.TryParse(parameter.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double baseValue))
            {
                return DependencyProperty.UnsetValue;
            }

            return baseValue * scale;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return WpfBinding.DoNothing;
        }
    }
}