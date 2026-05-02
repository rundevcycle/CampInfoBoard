using System.Windows;

namespace CampInfoBoard;

public partial class BoardNamePromptWindow : Window
{
    public string BoardName => BoardNameBox.Text.Trim();

    public BoardNamePromptWindow(string defaultName)
    {
        InitializeComponent();

        BoardNameBox.Text = defaultName;

        Loaded += (_, _) =>
        {
            BoardNameBox.Focus();
            BoardNameBox.SelectAll();
        };
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(BoardName))
        {
            WpfMessageBox.Show("Please enter a board name.", "Camp Info Board");
            return;
        }

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}