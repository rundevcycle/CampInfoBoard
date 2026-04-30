using CampInfoBoard.Models;
using System.Windows;

namespace CampInfoBoard
{
    public partial class ScheduleItemEditorWindow : Window
    {
        public ScheduleItem Item { get; }

        public ScheduleItemEditorWindow(ScheduleItem item)
        {
            InitializeComponent();

            Item = item;
            DataContext = Item;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}