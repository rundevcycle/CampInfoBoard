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
using Forms = System.Windows.Forms;

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
            DataContext = this;
            LoadData();
            Title = $"Camp Info Board - {AppPaths.CurrentBoardName}";
        }

        private void LoadData()
        {
            Data = DataService.LoadData();
            SortPhotosByDisplayOrder();
            SetupScheduleView();
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
            if (ScheduleGrid.SelectedItem is not ScheduleItem selectedItem)
            {
                WpfMessageBox.Show("Please select an event first.", "Camp Info Board");
                return;
            }

            if (WpfMessageBox.Show(
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
            if (AnnouncementGrid.SelectedItem is not Announcement selectedItem)
            {
                WpfMessageBox.Show("Please select an announcement first.", "Camp Info Board");
                return;
            }

            if (WpfMessageBox.Show(
                    "Delete the selected announcement?",
                    "Camp Info Board",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            Data.Announcements.Remove(selectedItem);
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
            if (PhotosGrid.SelectedItem is not PhotoItem selectedItem)
            {
                WpfMessageBox.Show("Please select a photo first.", "Camp Info Board");
                return;
            }

            if (WpfMessageBox.Show(
                    "Delete the selected photo?",
                    "Camp Info Board",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            Data.Photos.Remove(selectedItem);
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

            Data.Settings.AnnouncementRotationSeconds =
                Math.Max(1, Data.Settings.AnnouncementRotationSeconds);

            Data.Settings.PhotoRotationSeconds =
                Math.Max(1, Data.Settings.PhotoRotationSeconds);
        }

        private void RefreshDisplaySettingsBindings()
        {
            OnPropertyChanged(nameof(Data));
        }
    }
}