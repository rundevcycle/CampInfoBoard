using CampInfoBoard.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CampInfoBoard
{
    public partial class WeatherEditorWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public WeatherBlock Weather { get; }

        public Array WeatherPeriods => Enum.GetValues(typeof(WeatherPeriod));
        public Array WindDirections => Enum.GetValues(typeof(WindDirection));

        public DateTime Date
        {
            get => Weather.Date;
            set
            {
                Weather.Date = value.Date;
                OnPropertyChanged();
            }
        }

        public List<string> WeatherIcons { get; } =
        [
            "clear-night",
            "cloudy",
            "hazy-day",
            "light-cloud",
            "mostly-cloudy-day",
            "mostly-cloudy-night",
            "mostly-sunny-day",
            "rain",
            "showers-clear-night",
            "showers",
            "sun-and-cloud-day",
            "sun-and-showers-day",
            "sunny-day",
            "thunderstorm",
            "windy"
        ];


        public WeatherEditorWindow(WeatherBlock weather)
        {
            InitializeComponent();

            Weather = weather;
            DataContext = this;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string? SelectedIconPath =>
            string.IsNullOrWhiteSpace(Weather.Icon)
                ? null
                : $"pack://application:,,,/Assets/WeatherIcons/{Weather.Icon}.png";

        private void IconComboBox_SelectionChanged(
            object sender,
            System.Windows.Controls.SelectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(SelectedIconPath));

            if (string.IsNullOrWhiteSpace(Weather.Description))
            {
                Weather.Description = GetDefaultDescription(Weather.Icon);
                OnPropertyChanged(nameof(Weather));
            }
        }

        private static string GetDefaultDescription(string? icon) =>
            icon switch
            {
                "clear-night" => "Clear",
                "cloudy" => "Cloudy",
                "hazy-day" => "Hazy",
                "light-cloud" => "Light cloud",
                "mostly-cloudy-day" => "Mostly cloudy",
                "mostly-cloudy-night" => "Mostly cloudy",
                "mostly-sunny-day" => "Mainly sunny",
                "rain" => "Rain",
                "showers-clear-night" => "Showers, then clearing",
                "showers" => "Showers",
                "sun-and-cloud-day" => "Mix of sun and cloud",
                "sun-and-showers-day" => "Sunny with periods of showers",
                "sunny-day" => "Sunny",
                "thunderstorm" => "Thunderstorm",
                "windy" => "Windy",
                _ => ""
            };
    }
}