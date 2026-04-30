using CampInfoBoard.Models;
using CampInfoBoard.Services;
using CampInfoBoard.ViewModels;
using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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


        public Array TideLevels => Enum.GetValues(typeof(TideType));


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


        private string? PickImageFile()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }

            return null;
        }


        private void RecalculateTidesFromSelected_Click(object sender, RoutedEventArgs e)
        {
            if (TideGrid.SelectedItem is not TideEntry selectedTide)
            {
                MessageBox.Show("Please select a tide row first.", "Camp Info Board");
                return;
            }

            RecalculateTideLevelsFrom(selectedTide);

            TideGrid.Items.Refresh();
        }

        private void DeletePastTides_Click(object sender, RoutedEventArgs e)
        {
            DateTime cutoff = DateTime.Now.AddHours(-24);

            var oldEntries = Data.TideEntries
                .Where(t => t.Time < cutoff)
                .ToList();

            foreach (var tide in oldEntries)
            {
                Data.TideEntries.Remove(tide);
            }

            TideGrid.Items.Refresh();
        }

        private void RecalculateTideLevelsFrom(TideEntry anchor)
        {
            var ordered = Data.TideEntries
                .OrderBy(t => t.Time)
                .ToList();

            int anchorIndex = ordered.IndexOf(anchor);

            if (anchorIndex < 0)
            {
                return;
            }

            TideType nextLevel = anchor.TideLevel;

            for (int i = anchorIndex; i < ordered.Count; i++)
            {
                ordered[i].TideLevel = nextLevel;

                nextLevel = nextLevel == TideType.High
                    ? TideType.Low
                    : TideType.High;
            }

            nextLevel = anchor.TideLevel == TideType.High
                ? TideType.Low
                : TideType.High;

            for (int i = anchorIndex - 1; i >= 0; i--)
            {
                ordered[i].TideLevel = nextLevel;

                nextLevel = nextLevel == TideType.High
                    ? TideType.Low
                    : TideType.High;
            }
        }

        private void AddTide_Click(object sender, RoutedEventArgs e)
        {
            AddNextTideEntry();
        }

        private void TideGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    MoveToPreviousTideCell();
                }
                else
                {
                    MoveToNextTideCell();
                }
                e.Handled = true;
                return;
            }

            if (e.Key != Key.Enter)
            {
                return;
            }

            TideGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            TideGrid.CommitEdit(DataGridEditingUnit.Row, true);

            if (TideGrid.SelectedItem is TideEntry selectedTide)
            {
                var ordered = Data.TideEntries
                    .OrderBy(t => t.Time)
                    .ToList();

                if (ordered.LastOrDefault() == selectedTide)
                {
                    AddNextTideEntry();
                    e.Handled = true;
                }
            }
        }


        private void AddNextTideEntry()
        {
            var lastTide = Data.TideEntries
                .OrderByDescending(t => t.Time)
                .FirstOrDefault();

            var newTide = new TideEntry();

            if (lastTide == null)
            {
                newTide.Time = DateTime.Today;
                newTide.TideLevel = TideType.High;
            }
            else
            {
                newTide.Time = lastTide.Time.AddHours(6).AddMinutes(12);
                newTide.TideLevel = lastTide.TideLevel == TideType.High
                    ? TideType.Low
                    : TideType.High;
            }

            Data.TideEntries.Add(newTide);
            TideGrid.Items.Refresh();

            TideGrid.SelectedItem = newTide;
            TideGrid.CurrentCell = new DataGridCellInfo(newTide, TideGrid.Columns[0]);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                TideGrid.ScrollIntoView(newTide);
                TideGrid.Focus();
                TideGrid.BeginEdit();
            }));
        }


        private void MoveToNextTideCell()
        {
            TideGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            TideGrid.CommitEdit(DataGridEditingUnit.Row, true);

            if (TideGrid.SelectedItem is not TideEntry currentTide)
            {
                return;
            }

            int currentColumnIndex = TideGrid.CurrentColumn?.DisplayIndex ?? 0;
            int nextColumnIndex = currentColumnIndex + 1;

            if (nextColumnIndex >= TideGrid.Columns.Count)
            {
                nextColumnIndex = 0;
                MoveToNextTideRow();
                return;
            }

            TideGrid.CurrentCell = new DataGridCellInfo(
                currentTide,
                TideGrid.Columns[nextColumnIndex]);

            TideGrid.SelectedItem = currentTide;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                TideGrid.Focus();
                TideGrid.BeginEdit();
            }));
        }


        private void MoveToPreviousTideCell()
        {
            TideGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            TideGrid.CommitEdit(DataGridEditingUnit.Row, true);

            if (TideGrid.SelectedItem is not TideEntry currentTide)
            {
                return;
            }

            int currentColumnIndex = TideGrid.CurrentColumn?.DisplayIndex ?? 0;
            int previousColumnIndex = currentColumnIndex - 1;

            if (previousColumnIndex >= 0)
            {
                TideGrid.CurrentCell = new DataGridCellInfo(
                    currentTide,
                    TideGrid.Columns[previousColumnIndex]);

                TideGrid.SelectedItem = currentTide;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    TideGrid.Focus();
                    TideGrid.BeginEdit();
                }));

                return;
            }

            MoveToPreviousTideRow();
        }


        private void MoveToNextTideRow()
        {
            var ordered = Data.TideEntries
                .OrderBy(t => t.Time)
                .ToList();

            if (TideGrid.SelectedItem is not TideEntry currentTide)
            {
                return;
            }

            int currentIndex = ordered.IndexOf(currentTide);

            if (currentIndex < 0)
            {
                return;
            }

            TideEntry nextTide;

            if (currentIndex == ordered.Count - 1)
            {
                AddNextTideEntry();
                return;
            }
            else
            {
                nextTide = ordered[currentIndex + 1];
            }

            TideGrid.SelectedItem = nextTide;
            TideGrid.CurrentCell = new DataGridCellInfo(nextTide, TideGrid.Columns[0]);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                TideGrid.ScrollIntoView(nextTide);
                TideGrid.Focus();
                TideGrid.BeginEdit();
            }));
        }


        private void MoveToPreviousTideRow()
        {
            var ordered = Data.TideEntries
                .OrderBy(t => t.Time)
                .ToList();

            if (TideGrid.SelectedItem is not TideEntry currentTide)
            {
                return;
            }

            int currentIndex = ordered.IndexOf(currentTide);

            if (currentIndex <= 0)
            {
                return;
            }

            TideEntry previousTide = ordered[currentIndex - 1];

            TideGrid.SelectedItem = previousTide;
            TideGrid.CurrentCell = new DataGridCellInfo(
                previousTide,
                TideGrid.Columns[TideGrid.Columns.Count - 1]);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                TideGrid.ScrollIntoView(previousTide);
                TideGrid.Focus();
                TideGrid.BeginEdit();
            }));
        }


        private void PickTideDate_Click(object sender, RoutedEventArgs e)
        {
            if (TideGrid.SelectedItem is not TideEntry tide)
            {
                return;
            }

            var picker = new Calendar
            {
                SelectedDate = tide.Date,
                DisplayDate = tide.Date
            };

            var popup = new Window
            {
                Title = "Select Date",
                Content = picker,
                Width = 300,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            picker.SelectedDatesChanged += (_, _) =>
            {
                if (picker.SelectedDate.HasValue)
                {
                    tide.Date = picker.SelectedDate.Value;
                    popup.Close();
                    TideGrid.Items.Refresh();
                }
            };

            popup.ShowDialog();
        }


        private void EditingTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                textBox.Focus();
                textBox.SelectAll();
            }));
        }


        private void AddScheduleItem_Click(object sender, RoutedEventArgs e)
        {
            AddNextScheduleItem();
        }

        private void AddNextScheduleItem()
        {
            var lastItem = Data.Schedule
                .OrderByDescending(s => s.Start)
                .FirstOrDefault();

            var newItem = new ScheduleItem();

            if (lastItem == null)
            {
                newItem.Start = DateTime.Today.AddHours(9);
                newItem.End = newItem.Start.AddHours(1);
            }
            else
            {
                newItem.Start = lastItem.End;
                newItem.End = newItem.Start.AddHours(1);
                newItem.Location = lastItem.Location;
            }

            Data.Schedule.Add(newItem);
            ScheduleGrid.Items.Refresh();

            ScheduleGrid.SelectedItem = newItem;
            ScheduleGrid.CurrentCell = new DataGridCellInfo(newItem, ScheduleGrid.Columns[0]);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                ScheduleGrid.ScrollIntoView(newItem);
                ScheduleGrid.Focus();
                ScheduleGrid.BeginEdit();
            }));
        }


        private void ScheduleGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            ScheduleGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            ScheduleGrid.CommitEdit(DataGridEditingUnit.Row, true);

            if (ScheduleGrid.SelectedItem is ScheduleItem selectedItem)
            {
                var ordered = Data.Schedule
                    .OrderBy(s => s.Start)
                    .ToList();

                if (ordered.LastOrDefault() == selectedItem)
                {
                    AddNextScheduleItem();
                    e.Handled = true;
                }
            }
        }
    }
}