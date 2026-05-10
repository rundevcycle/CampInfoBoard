using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace CampInfoBoard.Converters
{
    public class EnumDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "";
            }

            string text = value.ToString() ?? "";

            return Regex.Replace(text, "(\\B[A-Z])", " $1");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return WpfBinding.DoNothing;
        }
    }
}