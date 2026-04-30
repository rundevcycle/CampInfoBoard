using CampInfoBoard.Models;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;

namespace CampInfoBoard;

public partial class PhotoEditorWindow : Window
{
    public PhotoItem Photo { get; }

    public PhotoEditorWindow(PhotoItem photo)
    {
        InitializeComponent();

        Photo = photo;

        ImagePathBox.Text = Photo.ImagePath;
        CaptionBox.Text = Photo.Caption;
        CreditBox.Text = Photo.Credit;
        ExpiryPicker.SelectedDate = Photo.ExpiryDate;
        ActiveCheckBox.IsChecked = Photo.IsActive;

        UpdatePreview();
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Photo",
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp|All Files|*.*"
        };

        if (dialog.ShowDialog(this) == true)
        {
            ImagePathBox.Text = dialog.FileName;
            UpdatePreview();
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Photo.ImagePath = ImagePathBox.Text.Trim();
        Photo.Caption = CaptionBox.Text.Trim();
        Photo.Credit = CreditBox.Text.Trim();
        Photo.ExpiryDate = ExpiryPicker.SelectedDate;
        Photo.IsActive = ActiveCheckBox.IsChecked == true;

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }



    private void UpdatePreview()
    {
        if (!File.Exists(ImagePathBox.Text))
        {
            PreviewImage.Source = null;
            return;
        }

        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(ImagePathBox.Text);
        image.DecodePixelWidth = 300;
        image.EndInit();

        PreviewImage.Source = image;
    }


}