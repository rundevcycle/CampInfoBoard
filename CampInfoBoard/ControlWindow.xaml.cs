using CampInfoBoard.Models;
using CampInfoBoard.Services;
using CampInfoBoard.ViewModels;
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Forms = System.Windows.Forms;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

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

        public List<string> DisplayMonitors =>
            Forms.Screen.AllScreens
                .Select((screen, index) =>
                    $"Monitor {index + 1}: {screen.Bounds.Width}x{screen.Bounds.Height}" +
                    (screen.Primary ? " (Primary)" : ""))
                .ToList();


        public ControlWindow()
        {
            InitializeComponent();

            AppPreferences preferences = AppPreferencesService.Load();

            if (AppPaths.GetAvailableBoards().Contains(preferences.LastBoardName))
            {
                AppPaths.CurrentBoardName = preferences.LastBoardName;
            }

            DataContext = this;

            LoadData();
            Title = $"Camp Info Board - {AppPaths.CurrentBoardName}";
        }

        private void LoadData()
        {
            Data = DataService.LoadData();
            NormalizePathsToCurrentBoard();  // Make paths in older boards relative to board root.
            SortPhotosByDisplayOrder();
            SetupScheduleView();
        }


        private void NormalizePathsToCurrentBoard()
        {
            foreach (var photo in Data.Photos)
            {
                photo.ImagePath = NormalizePath(photo.ImagePath, AppPaths.PhotosDirectory);
            }

            foreach (var announcement in Data.Announcements)
            {
                announcement.ImagePath = NormalizePath(announcement.ImagePath, AppPaths.AnnouncementsDirectory);
            }

            Data.BackgroundImagePath =
                NormalizePath(Data.BackgroundImagePath, AppPaths.BackgroundDirectory);
        }

        private string NormalizePath(string? path, string expectedFolder)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return "";
            }

            string resolvedPath = Path.GetFullPath(AppPaths.ResolveBoardPath(path));
            string expected = Path.GetFullPath(expectedFolder);

            if (resolvedPath.StartsWith(expected, StringComparison.OrdinalIgnoreCase))
            {
                return AppPaths.MakeBoardRelativePath(resolvedPath);
            }

            string fileName = Path.GetFileName(resolvedPath);

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                string rebasedPath = Path.Combine(expectedFolder, fileName);

                if (File.Exists(rebasedPath))
                {
                    return AppPaths.MakeBoardRelativePath(rebasedPath);
                }
            }

            return path;
        }


        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Sanity check before saving.
            NormalizeDisplaySettings();
            RefreshDisplaySettingsBindings();

            DataService.SaveData(Data);
            WpfMessageBox.Show("Saved.", "Camp Info Board");
        }


        private void SaveBoardAs_Click(object sender, RoutedEventArgs e)
        {
            var prompt = new BoardNamePromptWindow($"{AppPaths.CurrentBoardName} Copy")
            {
                Owner = this
            };

            if (prompt.ShowDialog() != true)
                return;

            try
            {
                DataService.SaveData(Data);

                AppPaths.SaveBoardAs(prompt.BoardName);
                AppPreferencesService.SaveLastBoardName(AppPaths.CurrentBoardName);
                LoadData();

                Title = $"Camp Info Board - {AppPaths.CurrentBoardName}";

                WpfMessageBox.Show(
                    $"Board saved as '{AppPaths.CurrentBoardName}'.",
                    "Camp Info Board");
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show(
                    ex.Message,
                    "Save Board As Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ExportBoard_Click(object sender, RoutedEventArgs e)
        {
            DataService.SaveData(Data);

            var dialog = new SaveFileDialog
            {
                Title = "Export Board Package",
                Filter = "Camp Info Board Package|*.ciboard|ZIP File|*.zip",
                FileName = $"{AppPaths.CurrentBoardName}.ciboard"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                AppPaths.ExportCurrentBoardPackage(dialog.FileName);

                WpfMessageBox.Show(
                    $"Board exported to:\n{dialog.FileName}",
                    "Camp Info Board");
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show(
                    ex.Message,
                    "Export Board Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private void ImportBoard_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Import Board Package",
                Filter = "Camp Info Board Package|*.ciboard;*.zip|All Files|*.*"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                string defaultBoardName = Path.GetFileNameWithoutExtension(dialog.FileName);

                DataService.SaveData(Data);

                while (true)
                {
                    var prompt = new BoardNamePromptWindow(defaultBoardName)
                    {
                        Owner = this
                    };

                    if (prompt.ShowDialog() != true)
                    {
                        return;
                    }

                    defaultBoardName = prompt.BoardName;

                    try
                    {
                        AppPaths.ImportBoardPackage(dialog.FileName, prompt.BoardName);
                        break;
                    }
                    catch (IOException ex) when (ex.Message.Contains("already exists"))
                    {
                        WpfMessageBox.Show(
                            "A board with that name already exists. Please choose a different name.",
                            "Camp Info Board");
                    }
                }

                LoadData();
                AppPreferencesService.SaveLastBoardName(AppPaths.CurrentBoardName);

                Title = $"Camp Info Board - {AppPaths.CurrentBoardName}";

                if (_displayWindow?.DataContext is DisplayViewModel viewModel)
                {
                    viewModel.ReloadData();
                    viewModel.RestartRotation();
                }

                WpfMessageBox.Show(
                    $"Board imported as '{AppPaths.CurrentBoardName}'.",
                    "Camp Info Board");
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show(
                    ex.Message,
                    "Import Board Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void OpenBoard_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenBoardWindow
            {
                Owner = this
            };

            if (dialog.ShowDialog() != true || dialog.SelectedBoardName == null)
            {
                return;
            }

            DataService.SaveData(Data);

            AppPaths.CurrentBoardName = dialog.SelectedBoardName;
            AppPreferencesService.SaveLastBoardName(AppPaths.CurrentBoardName);
            LoadData();

            Title = $"Camp Info Board - {AppPaths.CurrentBoardName}";

            if (_displayWindow?.DataContext is DisplayViewModel viewModel)
            {
                viewModel.ReloadData();
            }
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
                _displayWindow.Topmost = Data.Settings.DisplayAlwaysOnTop;
                PositionDisplayWindowOnSelectedMonitor();

                _displayWindow.Closed += (_, _) =>
                {
                    _displayWindow = null;
                    DisplayToggleButton.Content = "Show Display";
                };

                _displayWindow.Show();
                _displayWindow.WindowState = WindowState.Maximized;
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
                PositionDisplayWindowOnSelectedMonitor();

                _displayWindow.Topmost = Data.Settings.DisplayAlwaysOnTop;
                _displayWindow.Show();
                _displayWindow.WindowState = WindowState.Maximized;
                _displayWindow.Activate();

                DisplayToggleButton.Content = "Hide Display";
            }
        }


        private void RefreshDisplay_Click(object sender, RoutedEventArgs e)
        {
            NormalizeDisplaySettings();
            RefreshDisplaySettingsBindings();
            DataService.SaveData(Data);

            if (_displayWindow != null)
            {
                _displayWindow.Topmost = Data.Settings.DisplayAlwaysOnTop;
            }

            if (_displayWindow?.DataContext is DisplayViewModel viewModel)
            {
                viewModel.ReloadData();
                viewModel.RestartRotation();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            DataService.SaveData(Data);
            AppPreferencesService.SaveLastBoardName(AppPaths.CurrentBoardName);

            if (_displayWindow != null)
            {
                _displayWindow.Close();
                _displayWindow = null;
            }

            WpfApplication.Current.Shutdown();
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
            var dialog = new WpfOpenFileDialog
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


        private void ImportTidesCsv_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new WpfOpenFileDialog
            {
                Title = "Import Tides CSV",
                Filter = "CSV Files|*.csv|All Files|*.*"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            if (Data.TideEntries.Any())
            {
                MessageBoxResult choice = WpfMessageBox.Show(
                    "Replace existing tide entries?\n\nYes = Replace\nNo = Append\nCancel = Abort",
                    "Camp Info Board",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (choice == MessageBoxResult.Cancel)
                {
                    return;
                }

                if (choice == MessageBoxResult.Yes)
                {
                    Data.TideEntries.Clear();
                }
            }

            try
            {
                TideImportResult result = TideImportService.ImportFromCsv(dialog.FileName);

                foreach (TideEntry tide in result.ImportedTides)
                {
                    Data.TideEntries.Add(tide);
                }

                SortTidesByTime();

                WpfMessageBox.Show(
                    $"Imported {result.ImportedTides.Count} tide(s).\nSkipped {result.SkippedRows} row(s).",
                    "Camp Info Board");
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show(
                    ex.Message,
                    "Tide Import Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SortTidesByTime()
        {
            var orderedTides = Data.TideEntries
                .OrderBy(t => t.Time)
                .ToList();

            Data.TideEntries.Clear();

            foreach (TideEntry tide in orderedTides)
            {
                Data.TideEntries.Add(tide);
            }

            TideGrid.Items.Refresh();
        }

        private void RecalculateTidesFromSelected_Click(object sender, RoutedEventArgs e)
        {
            if (TideGrid.SelectedItem is not TideEntry selectedTide)
            {
                WpfMessageBox.Show("Please select a tide row first.", "Camp Info Board");
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

        private void TideGrid_PreviewKeyDown(object sender, WpfKeyEventArgs e)
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
            if (sender is not WpfTextBox textBox)
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
            var selectedItems = ScheduleGrid.SelectedItems
                .OfType<ScheduleItem>()
                .ToList();

            int count = selectedItems.Count;

            if (count == 0)
            {
                WpfMessageBox.Show("Please select one or more events first.", "Camp Info Board");
                return;
            }

            string label = count == 1 ? "event" : "events";

            if (WpfMessageBox.Show(
                    $"Delete {count} selected {label}?",
                    "Camp Info Board",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            foreach (ScheduleItem item in selectedItems)
            {
                Data.Schedule.Remove(item);
            }

            ScheduleView.Refresh();
        }

        private void EditSelectedScheduleItem()
        {
            if (ScheduleGrid.SelectedItem is not ScheduleItem selectedItem)
            {
                WpfMessageBox.Show("Please select an event first.", "Camp Info Board");
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
                WpfMessageBox.Show("Please select an event to copy.", "Camp Info Board");
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




        private void AddAnnouncement_Click(object sender, RoutedEventArgs e)
        {
            var item = new Announcement
            {
                Start = DateTime.Today,
                End = DateTime.Today.AddDays(1),
                Priority = 0
            };

            var editor = new AnnouncementEditorWindow(item)
            {
                Owner = this
            };

            if (editor.ShowDialog() == true)
            {
                if (!string.IsNullOrWhiteSpace(item.ImagePath))
                {
                    item.ImagePath = CopyAnnouncementImageToBoardFolder(item.ImagePath);
                }
                Data.Announcements.Add(item);
                AnnouncementGrid.Items.Refresh();
                AnnouncementGrid.SelectedItem = item;
            }
        }

        private void CopyAnnouncement_Click(object sender, RoutedEventArgs e)
        {
            if (AnnouncementGrid.SelectedItem is not Announcement selectedItem)
            {
                WpfMessageBox.Show("Please select an announcement to copy.", "Camp Info Board");
                return;
            }

            var copiedItem = CloneAnnouncement(selectedItem);

            var editor = new AnnouncementEditorWindow(copiedItem)
            {
                Owner = this
            };

            if (editor.ShowDialog() == true)
            {
                if (!string.IsNullOrWhiteSpace(copiedItem.ImagePath))
                {
                    copiedItem.ImagePath = CopyAnnouncementImageToBoardFolder(copiedItem.ImagePath);
                }
                Data.Announcements.Add(copiedItem);
                AnnouncementGrid.Items.Refresh();
                AnnouncementGrid.SelectedItem = copiedItem;
            }
        }

        private void EditAnnouncement_Click(object sender, RoutedEventArgs e)
        {
            EditSelectedAnnouncement();
        }

        private void AnnouncementGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditSelectedAnnouncement();
        }

        private void DeleteAnnouncement_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = AnnouncementGrid.SelectedItems
                .OfType<Announcement>()
                .ToList();

            int count = selectedItems.Count;

            if (count == 0)
            {
                WpfMessageBox.Show("Please select one or more announcements first.", "Camp Info Board");
                return;
            }

            string label = count == 1 ? "event" : "events";

            if (WpfMessageBox.Show(
                    $"Delete {count} selected {label}?",
                    "Camp Info Board",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            foreach (Announcement item in selectedItems)
            {
                Data.Announcements.Remove(item);
            }

            AnnouncementGrid.Items.Refresh();
        }


        private void EditSelectedAnnouncement()
        {
            if (AnnouncementGrid.SelectedItem is not Announcement selectedItem)
            {
                WpfMessageBox.Show("Please select an announcement first.", "Camp Info Board");
                return;
            }

            var editableCopy = CloneAnnouncement(selectedItem);

            var editor = new AnnouncementEditorWindow(editableCopy)
            {
                Owner = this
            };

            if (editor.ShowDialog() == true)
            {
                if (!string.IsNullOrWhiteSpace(editableCopy.ImagePath))
                {
                    editableCopy.ImagePath = CopyAnnouncementImageToBoardFolder(editableCopy.ImagePath);
                }

                CopyAnnouncement(editableCopy, selectedItem);
                AnnouncementGrid.Items.Refresh();
            }
        }

        private Announcement CloneAnnouncement(Announcement source)
        {
            return new Announcement
            {
                Title = source.Title,
                BodyText = source.BodyText,
                ImagePath = source.ImagePath,
                Start = source.Start,
                End = source.End,
                Priority = source.Priority
            };
        }

        private void CopyAnnouncement(Announcement source, Announcement target)
        {
            target.Title = source.Title;
            target.BodyText = source.BodyText;
            target.ImagePath = source.ImagePath;
            target.Start = source.Start;
            target.End = source.End;
            target.Priority = source.Priority;
        }





        private void AddPhoto_Click(object sender, RoutedEventArgs e)
        {
            var item = new PhotoItem
            {
                IsActive = true,
                DisplayOrder = Data.Photos.Count
            };

            var editor = new PhotoEditorWindow(item)
            {
                Owner = this
            };

            if (editor.ShowDialog() == true)
            {
                Data.Photos.Add(item);
                PhotosGrid.Items.Refresh();
            }
        }

        private void CopyPhoto_Click(object sender, RoutedEventArgs e)
        {
            if (PhotosGrid.SelectedItem is not PhotoItem selectedItem)
            {
                WpfMessageBox.Show("Please select a photo to copy.", "Camp Info Board");
                return;
            }

            var copiedItem = ClonePhoto(selectedItem);
            copiedItem.DisplayOrder = Data.Photos.Count;

            var editor = new PhotoEditorWindow(copiedItem)
            {
                Owner = this
            };

            if (editor.ShowDialog() == true)
            {
                Data.Photos.Add(copiedItem);
                PhotosGrid.Items.Refresh();
                PhotosGrid.SelectedItem = copiedItem;
            }
        }

        private void EditPhoto_Click(object sender, RoutedEventArgs e)
        {
            EditSelectedPhoto();
        }

        private void PhotosGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditSelectedPhoto();
        }

        private void DeletePhoto_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = PhotosGrid.SelectedItems
                .OfType<PhotoItem>()
                .ToList();

            int count = selectedItems.Count;

            if (count == 0)
            {
                WpfMessageBox.Show("Please select one or more photos first.", "Camp Info Board");
                return;
            }

            string label = count == 1 ? "photo" : "photos";

            if (WpfMessageBox.Show(
                    $"Delete {count} selected {label}?",
                    "Camp Info Board",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            foreach (PhotoItem item in selectedItems)
            {
                Data.Photos.Remove(item);
            }

            NormalizePhotoDisplayOrder();
            PhotosGrid.Items.Refresh();
        }

        private void EditSelectedPhoto()
        {
            if (PhotosGrid.SelectedItem is not PhotoItem selectedItem)
            {
                WpfMessageBox.Show("Please select a photo first.", "Camp Info Board");
                return;
            }

            var editableCopy = ClonePhoto(selectedItem);

            var editor = new PhotoEditorWindow(editableCopy)
            {
                Owner = this
            };

            if (editor.ShowDialog() == true)
            {
                CopyPhoto(editableCopy, selectedItem);
                PhotosGrid.Items.Refresh();
            }
        }

        private PhotoItem ClonePhoto(PhotoItem source)
        {
            return new PhotoItem
            {
                ImagePath = source.ImagePath,
                Caption = source.Caption,
                Credit = source.Credit,
                ExpiryDate = source.ExpiryDate,
                IsActive = source.IsActive,
                DisplayOrder = source.DisplayOrder
            };
        }

        private void CopyPhoto(PhotoItem source, PhotoItem target)
        {
            target.ImagePath = source.ImagePath;
            target.Caption = source.Caption;
            target.Credit = source.Credit;
            target.ExpiryDate = source.ExpiryDate;
            target.IsActive = source.IsActive;
            target.DisplayOrder = source.DisplayOrder;
        }

        private void SortPhotosByDisplayOrder()
        {
            var selectedPhoto = PhotosGrid.SelectedItem as PhotoItem;

            var orderedPhotos = Data.Photos
                .OrderBy(p => p.DisplayOrder)
                .ToList();

            Data.Photos.Clear();

            foreach (var photo in orderedPhotos)
            {
                Data.Photos.Add(photo);
            }

            NormalizePhotoDisplayOrder();

            PhotosGrid.Items.Refresh();

            if (selectedPhoto != null)
            {
                PhotosGrid.SelectedItem = selectedPhoto;
                PhotosGrid.ScrollIntoView(selectedPhoto);
            }
        }

        private void MovePhotoUp_Click(object sender, RoutedEventArgs e)
        {
            if (PhotosGrid.SelectedItem is not PhotoItem selectedItem)
            {
                WpfMessageBox.Show("Please select a photo first.", "Camp Info Board");
                return;
            }

            int index = Data.Photos.IndexOf(selectedItem);

            if (index <= 0)
                return;

            (Data.Photos[index - 1], Data.Photos[index]) =
                (Data.Photos[index], Data.Photos[index - 1]);

            NormalizePhotoDisplayOrder();

            PhotosGrid.Items.Refresh();
            PhotosGrid.SelectedItem = selectedItem;
            PhotosGrid.ScrollIntoView(selectedItem);
        }

        private void MovePhotoDown_Click(object sender, RoutedEventArgs e)
        {
            if (PhotosGrid.SelectedItem is not PhotoItem selectedItem)
            {
                WpfMessageBox.Show("Please select a photo first.", "Camp Info Board");
                return;
            }

            int index = Data.Photos.IndexOf(selectedItem);

            if (index < 0 || index >= Data.Photos.Count - 1)
                return;

            (Data.Photos[index], Data.Photos[index + 1]) =
                (Data.Photos[index + 1], Data.Photos[index]);

            NormalizePhotoDisplayOrder();

            PhotosGrid.Items.Refresh();
            PhotosGrid.SelectedItem = selectedItem;
            PhotosGrid.ScrollIntoView(selectedItem);
        }

        private void NormalizePhotoDisplayOrder()
        {
            for (int i = 0; i < Data.Photos.Count; i++)
            {
                Data.Photos[i].DisplayOrder = i;
            }
        }




        public int SelectedMonitorIndex
        {
            get => Data.Settings.DisplayMonitorIndex;
            set
            {
                if (Data.Settings.DisplayMonitorIndex != value)
                {
                    Data.Settings.DisplayMonitorIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        private void PositionDisplayWindowOnSelectedMonitor()
        {
            var screens = Forms.Screen.AllScreens;

            int index = SelectedMonitorIndex;

            if (index < 0 || index >= screens.Length)
            {
                index = 0;
            }

            var screen = screens[index];
            var window = _displayWindow!;

            window.WindowStartupLocation = WindowStartupLocation.Manual;

            window.WindowState = WindowState.Normal;

            window.Left = screen.Bounds.Left;
            window.Top = screen.Bounds.Top;
            window.Width = screen.Bounds.Width;
            window.Height = screen.Bounds.Height;
        }


        private void NormalizeDisplaySettings()
        {
            Data.Settings.ScheduleRotationSeconds =
                Math.Max(1, Data.Settings.ScheduleRotationSeconds);

            Data.Settings.ScheduleEventsPerPage =
                Math.Clamp(Data.Settings.ScheduleEventsPerPage, 1, 6);

            Data.Settings.WeatherRotationSeconds =
                Math.Max(1, Data.Settings.WeatherRotationSeconds);

            Data.Settings.AnnouncementRotationSeconds =
                Math.Max(1, Data.Settings.AnnouncementRotationSeconds);

            Data.Settings.PhotoRotationSeconds =
                Math.Max(1, Data.Settings.PhotoRotationSeconds);
        }

        private void RefreshDisplaySettingsBindings()
        {
            OnPropertyChanged(nameof(Data));
        }




        private void AddSunEntry_Click(object sender, RoutedEventArgs e)
        {
            SunEntry newEntry = CreateNextSunEntry();

            Data.SunEntries.Add(newEntry);
            SortSunEntriesByDate();

            SunGrid.SelectedItem = newEntry;
            SunGrid.CurrentCell = new DataGridCellInfo(newEntry, SunGrid.Columns[0]);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                SunGrid.ScrollIntoView(newEntry);
                SunGrid.Focus();
                SunGrid.BeginEdit();
            }));
        }

        private SunEntry CreateNextSunEntry()
        {
            SunEntry? lastEntry = Data.SunEntries
                .OrderByDescending(s => s.Date)
                .FirstOrDefault();

            if (lastEntry == null)
            {
                return new SunEntry();
            }

            DateTime nextDate = lastEntry.Date.AddDays(1);

            return new SunEntry
            {
                Date = nextDate,
                Sunrise = nextDate.Date.Add(lastEntry.Sunrise.TimeOfDay),
                Sunset = nextDate.Date.Add(lastEntry.Sunset.TimeOfDay)
            };
        }


        private void ImportSunCsv_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new WpfOpenFileDialog
            {
                Title = "Import Sunrise / Sunset CSV",
                Filter = "CSV Files|*.csv|All Files|*.*"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                SunImportResult result = SunImportService.ImportFromCsv(dialog.FileName);

                if (result.ImportedSunEntries.Count == 0)
                {
                    WpfMessageBox.Show(
                        $"No sun entries were imported.\nSkipped {result.SkippedRows} row(s).",
                        "Camp Info Board");

                    return;
                }

                if (Data.SunEntries.Any())
                {
                    MessageBoxResult choice = WpfMessageBox.Show(
                        "Replace existing sun entries?\n\nYes = Replace\nNo = Append\nCancel = Abort",
                        "Camp Info Board",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);

                    if (choice == MessageBoxResult.Cancel)
                    {
                        return;
                    }

                    if (choice == MessageBoxResult.Yes)
                    {
                        Data.SunEntries.Clear();
                    }
                }

                foreach (SunEntry entry in result.ImportedSunEntries)
                {
                    if (!Data.SunEntries.Any(s => s.Date.Date == entry.Date.Date))
                    {
                        Data.SunEntries.Add(entry);
                    }
                }

                SortSunEntriesByDate();

                int importedCount = result.ImportedSunEntries.Count;

                string entryLabel = importedCount == 1
                    ? "entry"
                    : "entries";

                WpfMessageBox.Show(
                    $"Imported {importedCount} sun {entryLabel}.\nSkipped {result.SkippedRows} row(s).",
                    "Camp Info Board");
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show(
                    ex.Message,
                    "Sun Import Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void DeletePastSun_Click(object sender, RoutedEventArgs e)
        {
            DateTime cutoff = DateTime.Today;

            var oldEntries = Data.SunEntries
                .Where(s => s.Date.Date < cutoff)
                .ToList();

            foreach (SunEntry entry in oldEntries)
            {
                Data.SunEntries.Remove(entry);
            }

            SunGrid.Items.Refresh();
        }

        private void PickSunDate_Click(object sender, RoutedEventArgs e)
        {
            if (SunGrid.SelectedItem is not SunEntry sun)
            {
                return;
            }

            var picker = new Calendar
            {
                SelectedDate = sun.Date,
                DisplayDate = sun.Date
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
                    DateTime selectedDate = picker.SelectedDate.Value.Date;

                    sun.Date = selectedDate;
                    sun.Sunrise = selectedDate.Add(sun.Sunrise.TimeOfDay);
                    sun.Sunset = selectedDate.Add(sun.Sunset.TimeOfDay);

                    popup.Close();
                    SunGrid.Items.Refresh();
                }
            };

            popup.ShowDialog();
        }

        private void SortSunEntriesByDate()
        {
            var orderedEntries = Data.SunEntries
                .OrderBy(s => s.Date)
                .ToList();

            Data.SunEntries.Clear();

            foreach (SunEntry entry in orderedEntries)
            {
                Data.SunEntries.Add(entry);
            }

            SunGrid.Items.Refresh();
        }

        private void SunGrid_PreviewKeyDown(object sender, WpfKeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    MoveToPreviousSunCell();
                }
                else
                {
                    MoveToNextSunCell();
                }

                e.Handled = true;
                return;
            }

            if (e.Key != Key.Enter)
            {
                return;
            }

            SunGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            SunGrid.CommitEdit(DataGridEditingUnit.Row, true);

            if (SunGrid.SelectedItem is SunEntry selectedSun)
            {
                var ordered = Data.SunEntries
                    .OrderBy(s => s.Date)
                    .ToList();

                if (ordered.LastOrDefault() == selectedSun)
                {
                    AddSunEntry_Click(sender, e);
                    e.Handled = true;
                }
            }
        }

        private void MoveToNextSunCell()
        {
            SunGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            SunGrid.CommitEdit(DataGridEditingUnit.Row, true);

            if (SunGrid.SelectedItem is not SunEntry currentSun)
            {
                return;
            }

            int currentColumnIndex = SunGrid.CurrentColumn?.DisplayIndex ?? 0;
            int nextColumnIndex = currentColumnIndex + 1;

            if (nextColumnIndex >= SunGrid.Columns.Count)
            {
                MoveToNextSunRow();
                return;
            }

            SunGrid.CurrentCell = new DataGridCellInfo(
                currentSun,
                SunGrid.Columns[nextColumnIndex]);

            SunGrid.SelectedItem = currentSun;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                SunGrid.Focus();
                SunGrid.BeginEdit();
            }));
        }

        private void MoveToPreviousSunCell()
        {
            SunGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            SunGrid.CommitEdit(DataGridEditingUnit.Row, true);

            if (SunGrid.SelectedItem is not SunEntry currentSun)
            {
                return;
            }

            int currentColumnIndex = SunGrid.CurrentColumn?.DisplayIndex ?? 0;
            int previousColumnIndex = currentColumnIndex - 1;

            if (previousColumnIndex >= 0)
            {
                SunGrid.CurrentCell = new DataGridCellInfo(
                    currentSun,
                    SunGrid.Columns[previousColumnIndex]);

                SunGrid.SelectedItem = currentSun;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    SunGrid.Focus();
                    SunGrid.BeginEdit();
                }));

                return;
            }

            MoveToPreviousSunRow();
        }

        private void MoveToNextSunRow()
        {
            var ordered = Data.SunEntries
                .OrderBy(s => s.Date)
                .ToList();

            if (SunGrid.SelectedItem is not SunEntry currentSun)
            {
                return;
            }

            int currentIndex = ordered.IndexOf(currentSun);

            if (currentIndex < 0)
            {
                return;
            }

            if (currentIndex == ordered.Count - 1)
            {
                AddSunEntry_Click(this, new RoutedEventArgs());
                return;
            }

            SunEntry nextSun = ordered[currentIndex + 1];

            SunGrid.SelectedItem = nextSun;
            SunGrid.CurrentCell = new DataGridCellInfo(nextSun, SunGrid.Columns[0]);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                SunGrid.ScrollIntoView(nextSun);
                SunGrid.Focus();
                SunGrid.BeginEdit();
            }));
        }

        private void MoveToPreviousSunRow()
        {
            var ordered = Data.SunEntries
                .OrderBy(s => s.Date)
                .ToList();

            if (SunGrid.SelectedItem is not SunEntry currentSun)
            {
                return;
            }

            int currentIndex = ordered.IndexOf(currentSun);

            if (currentIndex <= 0)
            {
                return;
            }

            SunEntry previousSun = ordered[currentIndex - 1];

            SunGrid.SelectedItem = previousSun;
            SunGrid.CurrentCell = new DataGridCellInfo(
                previousSun,
                SunGrid.Columns[SunGrid.Columns.Count - 1]);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                SunGrid.ScrollIntoView(previousSun);
                SunGrid.Focus();
                SunGrid.BeginEdit();
            }));
        }






        private void AddWeather_Click(object sender, RoutedEventArgs e)
        {
            WeatherBlock item = CreateNextWeatherBlock();

            var editor = new WeatherEditorWindow(item)
            {
                Owner = this
            };

            if (editor.ShowDialog() == true)
            {
                Data.Weather.Add(item);
                SortWeatherByDate();
                WeatherGrid.SelectedItem = item;
            }
        }

        private void EditWeather_Click(object sender, RoutedEventArgs e)
        {
            EditSelectedWeather();
        }

        private void WeatherGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditSelectedWeather();
        }

        private void CopyWeather_Click(object sender, RoutedEventArgs e)
        {
            if (WeatherGrid.SelectedItem is not WeatherBlock selectedItem)
            {
                WpfMessageBox.Show("Please select a weather row to copy.", "Camp Info Board");
                return;
            }

            WeatherBlock copiedItem = CloneWeatherBlock(selectedItem);

            copiedItem.Date = copiedItem.Period == WeatherPeriod.NightTime
                ? copiedItem.Date.AddDays(1)
                : copiedItem.Date;

            copiedItem.Period = copiedItem.Period == WeatherPeriod.DayTime
                ? WeatherPeriod.NightTime
                : WeatherPeriod.DayTime;

            var editor = new WeatherEditorWindow(copiedItem)
            {
                Owner = this
            };

            if (editor.ShowDialog() == true)
            {
                Data.Weather.Add(copiedItem);
                SortWeatherByDate();
                WeatherGrid.SelectedItem = copiedItem;
            }
        }

        private void DeleteWeather_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = WeatherGrid.SelectedItems
                .OfType<WeatherBlock>()
                .ToList();

            int count = selectedItems.Count;

            if (count == 0)
            {
                WpfMessageBox.Show("Please select one or more weather rows first.", "Camp Info Board");
                return;
            }

            string label = count == 1 ? "entry" : "entries";

            if (WpfMessageBox.Show(
                    $"Delete {count} selected weather {label}?",
                    "Camp Info Board",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            foreach (WeatherBlock item in selectedItems)
            {
                Data.Weather.Remove(item);
            }

            WeatherGrid.Items.Refresh();
        }

        private void EditSelectedWeather()
        {
            if (WeatherGrid.SelectedItem is not WeatherBlock selectedItem)
            {
                WpfMessageBox.Show("Please select a weather row first.", "Camp Info Board");
                return;
            }

            WeatherBlock editableCopy = CloneWeatherBlock(selectedItem);

            var editor = new WeatherEditorWindow(editableCopy)
            {
                Owner = this
            };

            if (editor.ShowDialog() == true)
            {
                CopyWeatherBlock(editableCopy, selectedItem);
                SortWeatherByDate();
                WeatherGrid.SelectedItem = selectedItem;
            }
        }

        private WeatherBlock CreateNextWeatherBlock()
        {
            WeatherBlock? lastBlock = Data.Weather
                .OrderByDescending(w => w.Date)
                .ThenByDescending(w => w.Period)
                .FirstOrDefault();

            if (lastBlock == null)
            {
                return new WeatherBlock();
            }

            if (lastBlock.Period == WeatherPeriod.DayTime)
            {
                return new WeatherBlock
                {
                    Date = lastBlock.Date,
                    Period = WeatherPeriod.NightTime
                };
            }

            return new WeatherBlock
            {
                Date = lastBlock.Date.AddDays(1),
                Period = WeatherPeriod.DayTime
            };
        }

        private WeatherBlock CloneWeatherBlock(WeatherBlock source)
        {
            return new WeatherBlock
            {
                Date = source.Date,
                Period = source.Period,
                Icon = source.Icon,
                TemperatureC = source.TemperatureC,
                FeelsLikeC = source.FeelsLikeC,
                WindSpeedKph = source.WindSpeedKph,
                WindDirectionValue = source.WindDirectionValue,
                WindGustKph = source.WindGustKph,
                UVIndex = source.UVIndex,
                Description = source.Description,
                PrecipitationDisplay = source.PrecipitationDisplay
            };
        }

        private void CopyWeatherBlock(WeatherBlock source, WeatherBlock target)
        {
            target.Date = source.Date;
            target.Period = source.Period;
            target.Icon = source.Icon;
            target.TemperatureC = source.TemperatureC;
            target.FeelsLikeC = source.FeelsLikeC;
            target.WindSpeedKph = source.WindSpeedKph;
            target.WindDirectionValue = source.WindDirectionValue;
            target.WindGustKph = source.WindGustKph;
            target.UVIndex = source.UVIndex;
            target.Description = source.Description;
            target.PrecipitationDisplay = source.PrecipitationDisplay;
        }

        private void SortWeatherByDate()
        {
            WeatherBlock? selectedWeather = WeatherGrid.SelectedItem as WeatherBlock;

            var orderedWeather = Data.Weather
                .OrderBy(w => w.Date)
                .ThenBy(w => w.Period)
                .ToList();

            Data.Weather.Clear();

            foreach (WeatherBlock weather in orderedWeather)
            {
                Data.Weather.Add(weather);
            }

            WeatherGrid.Items.Refresh();

            if (selectedWeather != null)
            {
                WeatherGrid.SelectedItem = selectedWeather;
                WeatherGrid.ScrollIntoView(selectedWeather);
            }
        }



        private void About_Click(object sender, RoutedEventArgs e)
        {
            var about = new AboutWindow
            {
                Owner = this
            };

            about.ShowDialog();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            DataService.SaveData(Data);
            Close();
        }


        private void NewBoard_Click(object sender, RoutedEventArgs e)
        {
            var prompt = new BoardNamePromptWindow("New Board")
            {
                Owner = this
            };

            if (prompt.ShowDialog() != true)
            {
                return;
            }

            try
            {
                DataService.SaveData(Data);

                AppPaths.CreateNewBoard(prompt.BoardName);
                AppPreferencesService.SaveLastBoardName(AppPaths.CurrentBoardName);
                LoadData();

                Title = $"Camp Info Board - {AppPaths.CurrentBoardName}";

                if (_displayWindow?.DataContext is DisplayViewModel viewModel)
                {
                    viewModel.ReloadData();
                    viewModel.RestartRotation();
                }
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show(
                    ex.Message,
                    "New Board Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }



        private void ChooseBackgroundImage_Click(object sender, RoutedEventArgs e)
        {
            string? path = PickImageFile();

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                Data.BackgroundImagePath = CopyBackgroundImageToBoardFolder(path);
                OnPropertyChanged(nameof(Data));
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show(
                    ex.Message,
                    "Background Image Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ClearBackgroundImage_Click(object sender, RoutedEventArgs e)
        {
            Data.BackgroundImagePath = "";
            OnPropertyChanged(nameof(Data));
        }

        private string CopyBackgroundImageToBoardFolder(string sourcePath)
        {
            AppPaths.EnsureFolders();

            string extension = Path.GetExtension(sourcePath);
            string fileName = $"background-{DateTime.Now:yyyyMMdd-HHmmssfff}{extension}";
            string targetPath = Path.Combine(AppPaths.BackgroundDirectory, fileName);

            File.Copy(sourcePath, targetPath, overwrite: false);

            return AppPaths.MakeBoardRelativePath(targetPath);
        }

        private void ChooseBackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            using Forms.ColorDialog dialog = new();

            if (dialog.ShowDialog() != Forms.DialogResult.OK)
            {
                return;
            }

            Data.Settings.BackgroundColor =
                $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";

            OnPropertyChanged(nameof(Data));
        }



        private string CopyAnnouncementImageToBoardFolder(string sourcePath)
        {
            AppPaths.EnsureFolders();

            string sourceFullPath = Path.GetFullPath(AppPaths.ResolveBoardPath(sourcePath));
            string announcementsFolder = Path.GetFullPath(AppPaths.AnnouncementsDirectory);

            if (sourceFullPath.StartsWith(announcementsFolder, StringComparison.OrdinalIgnoreCase))
            {
                return AppPaths.MakeBoardRelativePath(sourceFullPath);
            }

            string extension = Path.GetExtension(sourcePath);
            string fileName = $"announcement-{DateTime.Now:yyyyMMdd-HHmmssfff}{extension}";
            string targetPath = Path.Combine(AppPaths.AnnouncementsDirectory, fileName);

            File.Copy(sourceFullPath, targetPath, overwrite: false);

            return AppPaths.MakeBoardRelativePath(targetPath);
        }


        private void ScheduleGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                DeleteScheduleItem_Click(sender, e);
                e.Handled = true;
            }
        }

        private void WeatherGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                DeleteWeather_Click(sender, e);
                e.Handled = true;
            }
        }

        private void AnnouncementGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                DeleteAnnouncement_Click(sender, e);
                e.Handled = true;
            }
        }


        private void PhotosGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                DeletePhoto_Click(sender, e);
                e.Handled = true;
            }
        }

    }
}