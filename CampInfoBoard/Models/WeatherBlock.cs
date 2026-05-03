namespace CampInfoBoard.Models
{
    public class WeatherBlock
    {
        public DateTime Date { get; set; } = DateTime.Today;

        public WeatherPeriod Period { get; set; } = WeatherPeriod.DayTime;

        public string PeriodDisplay =>
            Period == WeatherPeriod.DayTime
                ? "Day"
                : "Night";

        public string? Icon { get; set; }

        // Internal storage = Metric
        public int? TemperatureC { get; set; }

        public int? FeelsLikeC { get; set; }

        public int? WindSpeedKph { get; set; }

        public WindDirection WindDirectionValue { get; set; } = WindDirection.Calm;

        public int? WindGustKph { get; set; }

        public int? UVIndex { get; set; }

        public bool HasUV => UVIndex != null;
        public string Description { get; set; } = "";

        public string PrecipitationDisplay { get; set; } = "";

        public bool IsPast => Date.Date < DateTime.Today;

        public string FormatTemperature(MeasurementDisplayMode mode)
        {
            if (TemperatureC == null)
            {
                return "";
            }

            int tempF = (int)Math.Round(
                (TemperatureC.Value * 9.0 / 5.0) + 32);

            return mode switch
            {
                MeasurementDisplayMode.Metric =>
                    $"{TemperatureC}°C",

                MeasurementDisplayMode.Imperial =>
                    $"{tempF}°F",

                MeasurementDisplayMode.Both =>
                    $"{TemperatureC}°C / {tempF}°F",

                _ =>
                    $"{TemperatureC}°C"
            };
        }

        public string FormatFeelsLike(MeasurementDisplayMode mode)
        {
            if (FeelsLikeC == null)
            {
                return "";
            }

            int tempF = (int)Math.Round(
                (FeelsLikeC.Value * 9.0 / 5.0) + 32);

            return mode switch
            {
                MeasurementDisplayMode.Metric =>
                    $"Feels like {FeelsLikeC}°C",

                MeasurementDisplayMode.Imperial =>
                    $"Feels like {tempF}°F",

                MeasurementDisplayMode.Both =>
                    $"Feels like {FeelsLikeC}°C / {tempF}°F",

                _ =>
                    $"Feels like {FeelsLikeC}°C"
            };
        }

        public string WindDirectionDisplay =>
            WindDirectionValue switch
            {
                WindDirection.Variable => "Variable",
                WindDirection.Calm => "Calm",
                _ => WindDirectionValue.ToString()
            };

        public string FormatWind(MeasurementDisplayMode mode)
        {
            if (WindSpeedKph == null)
            {
                return "";
            }

            int windMph = (int)Math.Round(WindSpeedKph.Value / 1.609);

            string speed = mode switch
            {
                MeasurementDisplayMode.Metric => $"{WindSpeedKph} km/h",
                MeasurementDisplayMode.Imperial => $"{windMph} mph",
                MeasurementDisplayMode.Both => $"{WindSpeedKph} km/h ({windMph} mph)",
                _ => $"{WindSpeedKph} km/h"
            };

            string gust = "";

            if (WindGustKph != null)
            {
                int gustMph = (int)Math.Round(WindGustKph.Value / 1.609);

                gust = mode switch
                {
                    MeasurementDisplayMode.Metric => $" gusting to {WindGustKph} km/h",
                    MeasurementDisplayMode.Imperial => $" gusting to {gustMph} mph",
                    MeasurementDisplayMode.Both => $" gusting to {WindGustKph} km/h ({gustMph} mph)",
                    _ => $" gusting to {WindGustKph} km/h"
                };
            }

            return WindDirectionValue == WindDirection.Calm
                ? $"Winds {speed}{gust}"
                : $"Winds {WindDirectionDisplay} {speed}{gust}";
        }


        public string? IconPath =>
            string.IsNullOrWhiteSpace(Icon)
                ? null
                : $"pack://application:,,,/Assets/WeatherIcons/{Icon}.png";

        public string WindDisplay => FormatWind(MeasurementDisplayMode.Metric);
    }
}