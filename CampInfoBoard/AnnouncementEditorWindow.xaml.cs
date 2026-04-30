using CampInfoBoard.Models;
using Microsoft.Win32;
using System.Windows;

namespace CampInfoBoard
{
    public partial class AnnouncementEditorWindow : Window
    {
        public Announcement Item { get; }

        public AnnouncementEditorWindow(Announcement item)
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

        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Announcement Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                Item.ImagePath = dialog.FileName;
            }
        }
    }
}