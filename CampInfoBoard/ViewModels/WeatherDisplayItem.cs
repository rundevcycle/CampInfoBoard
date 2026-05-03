using CampInfoBoard.Models;

namespace CampInfoBoard.ViewModels
{
    public class WeatherDisplayItem
    {
        private readonly WeatherBlock _weather;
        private readonly MeasurementDisplayMode _measurementMode;

        public WeatherDisplayItem(WeatherBlock weather, MeasurementDisplayMode measurementMode)
        {
            _weather = weather;
            _measurementMode = measurementMode;
        }

        public string DateLabel
        {
            get
            {
                DateTime today = DateTime.Today;

                if (_weather.Date.Date == today)
                {
                    return _weather.Period == WeatherPeriod.DayTime
                        ? "Today"
                        : "Tonight";
                }

                if (_weather.Date.Date == today.AddDays(1))
                {
                    return _weather.Period == WeatherPeriod.DayTime
                        ? "Tomorrow"
                        : "Tomorrow Night";
                }

                int dayOffset = (_weather.Date.Date - today).Days;

                return _weather.Period == WeatherPeriod.DayTime
                    ? $"Day {dayOffset}"
                    : $"Night {dayOffset}";
            }
        }

        public string PeriodDisplay => _weather.PeriodDisplay;

        public string? Icon => _weather.Icon;

        public string TemperatureDisplay => _weather.FormatTemperature(_measurementMode);

        public string FeelsLikeDisplay => _weather.FormatFeelsLike(_measurementMode);

        public string WindDisplay => _weather.FormatWind(_measurementMode);

        public bool HasWind => !string.IsNullOrWhiteSpace(WindDisplay);

        public int? UVIndex => _weather.UVIndex;

        public bool HasUV => _weather.HasUV;

        public string Description => _weather.Description;

        public string PrecipitationDisplay => _weather.PrecipitationDisplay;
    }
}