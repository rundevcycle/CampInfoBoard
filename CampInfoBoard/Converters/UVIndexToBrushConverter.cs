using System.Globalization;
using System.Windows.Data;
using Brushes = System.Windows.Media.Brushes;

namespace CampInfoBoard.Converters
{
    public class UVIndexToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int uv)
            {
                return Brushes.LightGray;
            }

            if (uv <= 2)
            {
                return Brushes.LimeGreen;
            }

            if (uv <= 5)
            {
                return Brushes.Gold;
            }

            if (uv <= 7)
            {
                return Brushes.Orange;
            }

            return Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}