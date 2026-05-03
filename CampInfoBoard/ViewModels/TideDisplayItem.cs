using CampInfoBoard.Models;

namespace CampInfoBoard.ViewModels
{
    public class TideDisplayItem
    {
        private readonly TideEntry _tide;
        public bool IsSeparator { get; }

        public string SeparatorLabel { get; }

        public TideDisplayItem(TideEntry tide, bool isSeparator = false, string separatorLabel = "")
        {
            _tide = tide;
            IsSeparator = isSeparator;
            SeparatorLabel = separatorLabel;
        }

        public string TimeDisplay => _tide.Time.ToString("h:mm tt");

        public string IconPath =>
            _tide.TideLevel == TideType.High
                ? "pack://application:,,,/Assets/WeatherIcons/tide-high-2.png"
                : "pack://application:,,,/Assets/WeatherIcons/tide-low-2.png";
    }
}