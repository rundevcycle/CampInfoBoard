using CampInfoBoard.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using CampInfoBoard.Services;
using System.IO;

namespace CampInfoBoard.ViewModels;

public class DisplayViewModel : INotifyPropertyChanged
{
    private readonly DispatcherTimer _clockTimer;
    private readonly DispatcherTimer _rotationTimer;

    private DisplayMode _mode = DisplayMode.Schedule;
    private int _announcementIndex = 0;
    private int _photoIndex = 0;

    public event PropertyChangedEventHandler? PropertyChanged;

    public DisplaySettings Settings { get; set; } = new();
    public RadioInfo Radio { get; set; } = new();

    public ObservableCollection<WeatherBlock> Weather { get; } = new();
    public ObservableCollection<ScheduleItem> Schedule { get; } = new();
    public ObservableCollection<Announcement> Announcements { get; } = new();
    public ObservableCollection<PhotoItem> Photos { get; } = new();
    public ObservableCollection<TideEntry> TideEntries { get; } = new();

    public string BackgroundImagePath { get; set; } = "";

    public string UVDisplay { get; set; } = "UV 6";

    public TideInfo Tide { get; set; } = new()
    {
        HighDisplay = "High 2:35 PM",
        LowDisplay = "Low 8:50 PM"
    };

    public SunInfo Sun { get; set; } = new()
    {
        SunriseDisplay = "Sunrise 5:42 AM",
        SunsetDisplay = "Sunset 8:57 PM"
    };

    private DateTime _currentTime = DateTime.Now;
    public DateTime CurrentTime
    {
        get => _currentTime;
        set { _currentTime = value; OnPropertyChanged(); OnPropertyChanged(nameof(FilteredSchedule)); }
    }

    public IEnumerable<ScheduleItem> FilteredSchedule =>
        Schedule
            .Where(x => x.End >= DateTime.Now && x.Start <= DateTime.Now.AddHours(24))
            .OrderBy(x => x.Start);

    public IEnumerable<Announcement> ActiveAnnouncements =>
        Announcements
            .Where(x =>
                (x.Start == null || x.Start <= DateTime.Now) &&
                (x.End == null || x.End >= DateTime.Now))
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.Start ?? DateTime.MinValue);


    // Cache photos to minimize File checks.
    private List<PhotoItem> _activePhotos = new();

    public IEnumerable<PhotoItem> ActivePhotos => _activePhotos;

    public void RefreshActivePhotos()
    {
        _activePhotos = Photos
            .Where(p =>
                p.IsActive &&
                !p.IsExpired &&
                !string.IsNullOrWhiteSpace(p.ImagePath) &&
                File.Exists(p.ImagePath))
            .OrderBy(p => p.DisplayOrder)
            .ToList();

        OnPropertyChanged(nameof(ActivePhotos));
    }


    public Announcement? CurrentAnnouncement => ActiveAnnouncements.ElementAtOrDefault(_announcementIndex);
    public PhotoItem? CurrentPhoto => ActivePhotos.ElementAtOrDefault(_photoIndex);

    public TideEntry? NextHighTide => 
        TideEntries
            .Where(t => t.TideLevel == TideType.High && t.Time >= DateTime.Now)
            .OrderBy(t => t.Time)
            .FirstOrDefault();

    public TideEntry? NextLowTide =>
        TideEntries
            .Where(t => t.TideLevel == TideType.Low && t.Time >= DateTime.Now)
            .OrderBy(t => t.Time)
            .FirstOrDefault();

    public string HighTideDisplay => NextHighTide == null ? "" : $"High {NextHighTide.Time:h:mm tt}";
    public string LowTideDisplay => NextLowTide == null ? "" : $"Low {NextLowTide.Time:h:mm tt}";

    public bool IsScheduleVisible => _mode == DisplayMode.Schedule;
    public bool IsAnnouncementVisible => _mode == DisplayMode.Announcement;
    public bool IsPhotoVisible => _mode == DisplayMode.Photo;

    public int ScheduleSeconds { get; set; } = 5; // TODO make configurable = 30;
    public int AnnouncementSeconds { get; set; } = 5;  // TODO make configurable = 12;
    public int PhotoSeconds { get; set; } = 5;  // TODO make configurable = 15;

    public DisplayViewModel()
    {
        LoadAppData();

        _clockTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _clockTimer.Tick += (_, _) =>
        {
            CurrentTime = DateTime.Now;
            OnPropertyChanged(nameof(NextHighTide));
            OnPropertyChanged(nameof(NextLowTide));
            OnPropertyChanged(nameof(HighTideDisplay));
            OnPropertyChanged(nameof(LowTideDisplay));
        };
        _clockTimer.Start();

        _rotationTimer = new DispatcherTimer();
        _rotationTimer.Tick += (_, _) => AdvanceMode();

        SetMode(DisplayMode.Schedule, ScheduleSeconds);
    }

    private void AdvanceMode()
    {
        if (_mode == DisplayMode.Schedule)
        {
            if (ActiveAnnouncements.Any())
            {
                _announcementIndex = 0;
                SetMode(DisplayMode.Announcement, AnnouncementSeconds);
            }
            else if (ActivePhotos.Any())
            {
                _photoIndex = 0;
                SetMode(DisplayMode.Photo, PhotoSeconds);
            }
            else
            {
                SetMode(DisplayMode.Schedule, ScheduleSeconds);
            }

            return;
        }

        if (_mode == DisplayMode.Announcement)
        {
            _announcementIndex++;

            if (_announcementIndex < ActiveAnnouncements.Count())
            {
                OnPropertyChanged(nameof(CurrentAnnouncement));
                SetMode(DisplayMode.Announcement, AnnouncementSeconds);
            }
            else if (ActivePhotos.Any())
            {
                _photoIndex = 0;
                SetMode(DisplayMode.Photo, PhotoSeconds);
            }
            else
            {
                SetMode(DisplayMode.Schedule, ScheduleSeconds);
            }

            return;
        }

        if (_mode == DisplayMode.Photo)
        {
            _photoIndex++;

            if (_photoIndex < ActivePhotos.Count())
            {
                OnPropertyChanged(nameof(CurrentPhoto));
                SetMode(DisplayMode.Photo, PhotoSeconds);
            }
            else
            {
                SetMode(DisplayMode.Schedule, ScheduleSeconds);
            }
        }
    }

    private void SetMode(DisplayMode mode, int seconds)
    {
        _mode = mode;

        OnPropertyChanged(nameof(IsScheduleVisible));
        OnPropertyChanged(nameof(IsAnnouncementVisible));
        OnPropertyChanged(nameof(IsPhotoVisible));
        OnPropertyChanged(nameof(CurrentAnnouncement));
        OnPropertyChanged(nameof(CurrentPhoto));
        OnPropertyChanged(nameof(FilteredSchedule));

        _rotationTimer.Stop();
        _rotationTimer.Interval = TimeSpan.FromSeconds(seconds);
        _rotationTimer.Start();
    }


    private void LoadAppData()
    {
        var data = DataService.LoadData();

        Settings = data.Settings;
        Radio = data.Radio;
        Sun = data.Sun;
        UVDisplay = data.UVDisplay;
        BackgroundImagePath = data.BackgroundImagePath;

        Weather.Clear();
        foreach (var item in data.Weather)
        {
            Weather.Add(item);
        }

        TideEntries.Clear();
        foreach (var tide in data.TideEntries)
        {
            TideEntries.Add(tide);
        }

        Schedule.Clear();
        foreach (var item in data.Schedule)
        {
            Schedule.Add(item);
        }

        Announcements.Clear();
        foreach (var item in data.Announcements)
        {
            Announcements.Add(item);
        }

        Photos.Clear();
        foreach (var item in data.Photos)
        {
            Photos.Add(item);
        }
        RefreshActivePhotos();
    }


    public void ReloadData()
    {
        LoadAppData();

        OnPropertyChanged(nameof(Settings));
        OnPropertyChanged(nameof(Radio));
        OnPropertyChanged(nameof(Sun));
        OnPropertyChanged(nameof(UVDisplay));
        OnPropertyChanged(nameof(BackgroundImagePath));
        OnPropertyChanged(nameof(FilteredSchedule));
        OnPropertyChanged(nameof(HighTideDisplay));
        OnPropertyChanged(nameof(LowTideDisplay));
        OnPropertyChanged(nameof(ActivePhotos));
        OnPropertyChanged(nameof(CurrentPhoto));
    }


    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
