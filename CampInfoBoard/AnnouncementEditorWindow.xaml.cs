using CampInfoBoard.Models;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using Image = System.Windows.Controls.Image;

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
            var dialog = new WpfOpenFileDialog
            {
                Title = "Select Announcement Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                Item.ImagePath = dialog.FileName;
                BindingOperations.GetBindingExpression(ImagePathText, TextBlock.TextProperty)?.UpdateTarget();
                BindingOperations.GetBindingExpression(ImagePreview, Image.SourceProperty)?.UpdateTarget();
            }
        }

        private void ClearImage_Click(object sender, RoutedEventArgs e)
        {
            Item.ImagePath = "";

            BindingOperations.GetBindingExpression(ImagePathText, TextBlock.TextProperty)?.UpdateTarget();
            BindingOperations.GetBindingExpression(ImagePreview, Image.SourceProperty)?.UpdateTarget();
        }
    }
}