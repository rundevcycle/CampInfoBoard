using CampInfoBoard.Models;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace CampInfoBoard
{
    public partial class ScheduleItemEditorWindow : Window
    {
        private readonly Action<ScheduleItem>? _saveAndNewAction;

        public ScheduleItem Item { get; }

        public ScheduleItemEditorWindow(
            ScheduleItem item,
            Action<ScheduleItem>? saveAndNewAction = null)
        {
            InitializeComponent();

            Item = item;
            _saveAndNewAction = saveAndNewAction;

            DataContext = Item;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartDateBox.Focus();
            StartDateBox.SelectAll();
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is WpfTextBox textBox)
            {
                textBox.SelectAll();
            }
        }

        private void TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not WpfTextBox textBox)
            {
                return;
            }

            if (textBox.IsKeyboardFocusWithin)
            {
                return;
            }

            e.Handled = true;
            textBox.Focus();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            UpdateTextBoxBindings();

            DialogResult = true;
            Close();
        }

        private void SaveAndNew_Click(object sender, RoutedEventArgs e)
        {
            UpdateTextBoxBindings();

            ScheduleItem savedItem = CloneScheduleItem(Item);

            _saveAndNewAction?.Invoke(savedItem);

            PrepareNextItem(savedItem);

            DataContext = null;
            DataContext = Item;

            StartDateBox.Focus();
            StartDateBox.SelectAll();
        }

        private void PrepareNextItem(ScheduleItem savedItem)
        {
            TimeSpan duration = savedItem.End > savedItem.Start
                ? savedItem.End - savedItem.Start
                : TimeSpan.FromHours(1);

            DateTime nextStart = savedItem.End > savedItem.Start
                ? savedItem.End
                : savedItem.Start.AddHours(1);

            Item.Start = nextStart;
            Item.End = nextStart.Add(duration);

            Item.Title = "";
            Item.Location = savedItem.Location;
            Item.Speaker = "";
            Item.Description = "";
        }

        private static ScheduleItem CloneScheduleItem(ScheduleItem source)
        {
            return new ScheduleItem
            {
                Start = source.Start,
                End = source.End,
                HasEndTime = source.HasEndTime,
                Title = source.Title,
                Location = source.Location,
                Speaker = source.Speaker,
                Description = source.Description
            };
        }

        private void UpdateTextBoxBindings()
        {
            UpdateTextBoxBindings(this);
        }

        private static void UpdateTextBoxBindings(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is WpfTextBox textBox)
                {
                    BindingExpression? binding =
                        textBox.GetBindingExpression(WpfTextBox.TextProperty);

                    binding?.UpdateSource();
                }

                UpdateTextBoxBindings(child);
            }
        }
    }
}