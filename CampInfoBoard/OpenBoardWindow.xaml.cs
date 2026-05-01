using CampInfoBoard.Services;
using System.Windows;
using System.Windows.Input;

namespace CampInfoBoard;

public partial class OpenBoardWindow : Window
{
    public string? SelectedBoardName => BoardsList.SelectedItem as string;

    public OpenBoardWindow()
    {
        InitializeComponent();

        BoardsList.ItemsSource = AppPaths.GetAvailableBoards();
        BoardsList.SelectedItem = AppPaths.CurrentBoardName;

        Loaded += (_, _) => BoardsList.Focus();
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedBoardName == null)
        {
            MessageBox.Show("Please select a board.", "Camp Info Board");
            return;
        }

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void BoardsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (SelectedBoardName != null)
        {
            DialogResult = true;
        }
    }
}