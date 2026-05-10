using CampInfoBoard.Models;
using CampInfoBoard.ViewModels;
using System.Windows;

namespace CampInfoBoard
{
    /// <summary>
    /// Interaction logic for DisplayWindow.xaml
    /// </summary>
    public partial class DisplayWindow : Window
    {
        public DisplayWindow()
        {
            InitializeComponent();
            DataContext = new DisplayViewModel();
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            if (DataContext is not DisplayViewModel viewModel)
            {
                return;
            }

            string themePath = viewModel.Settings.Theme switch
            {
                DisplayTheme.Light => "/Themes/LightTheme.xaml",
                DisplayTheme.DarkHighContrast => "/Themes/DarkHighContrastTheme.xaml",
                DisplayTheme.LightHighContrast => "/Themes/LightHighContrastTheme.xaml",
                _ => "/Themes/DarkTheme.xaml"
            };

            ResourceDictionary themeDictionary = new()
            {
                Source = new Uri(themePath, UriKind.Relative)
            };

            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(themeDictionary);
        }


        public void RefreshTheme()
        {
            ApplyTheme();
        }
    }
}
