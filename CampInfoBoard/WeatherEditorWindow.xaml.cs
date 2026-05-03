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
    }
}