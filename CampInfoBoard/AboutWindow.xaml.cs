using System.Reflection;
using System.Windows;

namespace CampInfoBoard
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            Version? version = Assembly.GetExecutingAssembly().GetName().Version;
            VersionText.Text = $"Version {version}";
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}