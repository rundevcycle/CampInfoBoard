using CampInfoBoard.Models;
using CampInfoBoard.Services;
using CampInfoBoard.ViewModels;
using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CampInfoBoard
{
    public partial class ControlWindow : Window, INotifyPropertyChanged
    {
        private DisplayWindow? _displayWindow;

        private AppData _data = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICollectionView ScheduleView { get; private set; }

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
            SetupScheduleView();
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


        private void SetupScheduleView()
        {
            ScheduleView = CollectionViewSource.GetDefaultView(Data.Schedule);

            ScheduleView.SortDescriptions.Clear();
            ScheduleView.SortDescriptions.Add(
                new SortDescription(nameof(ScheduleItem.Start), ListSortDirection.Ascending));

            OnPropertyChanged(nameof(ScheduleView));
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




        private ScheduleItem CloneScheduleItem(ScheduleItem source)
        {
            return new ScheduleItem
            {
                Start = source.Start,
                End = source.End,
                Title = source.Title,
                Location = source.Location,
                Speaker = source.Speaker,
                Description = source.Description
            };
        }

        private void CopyScheduleItem(ScheduleItem source, ScheduleItem target)
        {
            target.Start = source.Start;
            target.End = source.End;
            target.Title = source.Title;
            target.Location = source.Location;
            target.Speaker = source.Speaker;
            target.Description = source.Description;
        }

        private void AddScheduleItem_Click(object sender, RoutedEventArgs e)
        {
            var item = CreateNextScheduleItem();

            var editor = new ScheduleItemEditorWindow(item)
            {
                Owner = this
            };

            if (editor.ShowDialog() == true)
            {
                Data.Schedule.Add(item);
                ScheduleView.Refresh();
                ScheduleGrid.SelectedItem = item;
            }
        }

        private void EditScheduleItem_Click(object sender, RoutedEventArgs e)
        {
            EditSelectedScheduleItem();
        }

        private void ScheduleGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditSelectedScheduleItem();
        }

        private void DeleteScheduleItem_Click(object sender, RoutedEventArgs e)
        {
            if (ScheduleGrid.SelectedItem is not ScheduleItem selectedItem)
            {
                MessageBox.Show("Please select an event first.", "Camp Info Board");
                return;
            }

            if (MessageBox.Show(
                    "Delete the selected event?",
                    "Camp Info Board",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            Data.Schedule.Remove(selectedItem);
            ScheduleView.Refresh();
        }

        private void EditSelectedScheduleItem()
        {
            if (ScheduleGrid.SelectedItem is not ScheduleItem selectedItem)
            {
                MessageBox.Show("Please select an event first.", "Camp Info Board");
                return;
            }

            var editableCopy = CloneScheduleItem(selectedItem);

            var editor = new ScheduleItemEditorWindow(editableCopy)
            {
                Owner = this
            };

            if (editor.ShowDialog() == true)
            {
                CopyScheduleItem(editableCopy, selectedItem);
                ScheduleView.Refresh();
            }
        }

        private ScheduleItem CreateNextScheduleItem()
        {
            return new ScheduleItem
            {
                Start = DateTime.Today,
                End = DateTime.Today.AddHours(1),
                Title = "",
                Location = "",
                Speaker = "",
                Description = ""
            };
        }

        private void CopyScheduleItem_Click(object sender, RoutedEventArgs e)
        {
            if (ScheduleGrid.SelectedItem is not ScheduleItem selectedItem)
            {
                MessageBox.Show("Please select an event to copy.", "Camp Info Board");
                return;
            }

            ScheduleItem copiedItem = CloneScheduleItem(selectedItem);

            copiedItem.Start = copiedItem.Start.AddDays(1);
            copiedItem.End = copiedItem.End.AddDays(1);

            var editor = new ScheduleItemEditorWindow(copiedItem)
            {
                Owner = this
            };

            if (editor.ShowDialog() == true)
            {
                Data.Schedule.Add(copiedItem);
                ScheduleView.Refresh();
                ScheduleGrid.SelectedItem = copiedItem;
            }
        }

    }
}