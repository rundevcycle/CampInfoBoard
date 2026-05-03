using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace CampInfoBoard.Converters
{
    public class ScheduleBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isNow && isNow)
            {
                return new SolidColorBrush(Color.FromArgb(58, 28, 48, 72));
            }

            return new SolidColorBrush(Color.FromArgb(45, 0, 0, 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}