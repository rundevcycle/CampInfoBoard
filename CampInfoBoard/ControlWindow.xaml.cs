using CampInfoBoard.Models;
using CampInfoBoard.Services;
using CampInfoBoard.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CampInfoBoard
{
    public partial class ControlWindow : Window, INotifyPropertyChanged
    {
        private DisplayWindow? _displayWindow;

        private AppData _data = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public AppData Data
        {
            get => _data;
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        public ControlWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadData();
        }

        private void LoadData()
        {
            Data = DataService.LoadData();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DataService.SaveData(Data);
            MessageBox.Show("Saved.", "Camp Info Board");
        }

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void DisplayToggle_Click(object sender, RoutedEventArgs e)
        {
            if (_displayWindow == null)
            {
                _displayWindow = new DisplayWindow();

                _displayWindow.Closed += (_, _) =>
                {
                    _displayWindow = null;
                    DisplayToggleButton.Content = "Show Display";
                };

                _displayWindow.Show();
                DisplayToggleButton.Content = "Hide Display";

                return;
            }

            if (_displayWindow.IsVisible)
            {
                _displayWindow.Hide();
                DisplayToggleButton.Content = "Show Display";
            }
            else
            {
                _displayWindow.Show();
                _displayWindow.WindowState = WindowState.Maximized;
                _displayWindow.Activate();

                DisplayToggleButton.Content = "Hide Display";
            }
        }


        private void RefreshDisplay_Click(object sender, RoutedEventArgs e)
        {
            DataService.SaveData(Data);

            if (_displayWindow?.DataContext is DisplayViewModel viewModel)
            {
                viewModel.ReloadData();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_displayWindow != null)
            {
                _displayWindow.Close();
                _displayWindow = null;
            }

            Application.Current.Shutdown();
        }
    }
}