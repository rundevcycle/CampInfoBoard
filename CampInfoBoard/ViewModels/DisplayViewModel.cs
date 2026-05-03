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
    public ObservableCollection<SunEntry> SunEntries { get; } = new();

    public string BackgroundImagePath { get; set; } = "";



    public IEnumerable<WeatherDisplayItem> DisplayWeather =>
        GetWeatherDisplaySlots()
            .Select(slot => Weather.FirstOrDefault(w =>
                w.Date.Date == slot.Date.Date &&
                w.Period == slot.Period))
            .Where(w => w != null)
            .Cast<WeatherBlock>()
            .Select(w => new WeatherDisplayItem(w, Settings.MeasurementMode));


    private IEnumerable<(DateTime Date, WeatherPeriod Period)> GetWeatherDisplaySlots()
    {
        DateTime now = DateTime.Now;
        DateTime today = now.Date;

        DateTime startDate;
        WeatherPeriod startPeriod;

        // Change first period at 4 am and 5 pm.
        if (now.TimeOfDay < TimeSpan.FromHours(4))
        {
            startDate = today.AddDays(-1);
            startPeriod = WeatherPeriod.NightTime;
        }
        else if (now.TimeOfDay < TimeSpan.FromHours(17))
        {
            startDate = today;
            startPeriod = WeatherPeriod.DayTime;
        }
        else
        {
            startDate = today;
            startPeriod = WeatherPeriod.NightTime;
        }

        DateTime date = startDate;
        WeatherPeriod period = startPeriod;

        for (int i = 0; i < 3; i++)
        {
            yield return (date, period);

            if (period == WeatherPeriod.DayTime)
            {
                period = WeatherPeriod.NightTime;
            }
            else
            {
                date = date.AddDays(1);
                period = WeatherPeriod.DayTime;
            }
        }
    }

    public TideInfo Tide { get; set; } = new()
    {
        HighDisplay = "High 2:35 PM",
        LowDisplay = "Low 8:50 PM"
    };

    public IEnumerable<TideDisplayItem> DisplayTides
    {
        get
        {
            var tides = TideEntries
                .Where(t => t.Time >= DateTime.Now.AddHours(-1))
                .OrderBy(t => t.Time)
                .Take(3)
                .ToList();

            List<TideDisplayItem> result = new();

            DateTime? currentDate = null;

            foreach (var tide in tides)
            {
                if (currentDate != tide.Time.Date)
                {
                    string label = tide.Time.Date == DateTime.Today.AddDays(1)
                        ? "Tomorrow"
                        : tide.Time.Date > DateTime.Today.AddDays(1)
                            ? tide.Time.ToString("MMM d")
                            : "";

                    if (!string.IsNullOrWhiteSpace(label))
                    {
                        result.Add(new TideDisplayItem(tide, true, label));
                    }
                    currentDate = tide.Time.Date;
                }

                result.Add(new TideDisplayItem(tide));
            }

            return result;
        }
    }

    public SunEntry? TodaySun =>
        SunEntries
            .Where(s => s.Date.Date == DateTime.Today)
            .OrderBy(s => s.Date)
            .FirstOrDefault();

    public string SunriseDisplay =>
        TodaySun == null ? "" : $"Sunrise {TodaySun.Sunrise:h:mm tt}";

    public string SunsetDisplay =>
        TodaySun == null ? "" : $"Sunset {TodaySun.Sunset:h:mm tt}";


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
    public Announcement? CurrentAnnouncement => ActiveAnnouncements.ElementAtOrDefault(_announcementIndex);




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

    public PhotoItem? CurrentPhoto
    {
        get
        {
            return ActivePhotos.ElementAtOrDefault(_photoIndex);
        }
    }
    public bool HasCurrentPhotoText =>
        CurrentPhoto != null &&
        (!string.IsNullOrWhiteSpace(CurrentPhoto.Caption) ||
         !string.IsNullOrWhiteSpace(CurrentPhoto.Credit));


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
    public bool IsWeatherVisible => _mode == DisplayMode.Weather;
    public bool IsAnnouncementVisible => _mode == DisplayMode.Announcement;
    public bool IsPhotoVisible => _mode == DisplayMode.Photo;
    public bool IsPhotoFallbackVisible => IsPhotoVisible && CurrentPhoto == null;

    public int ScheduleSeconds => Math.Max(1, Settings.ScheduleRotationSeconds);
    public int WeatherSeconds => Math.Max(1, Settings.WeatherRotationSeconds);
    public int AnnouncementSeconds => Math.Max(1, Settings.AnnouncementRotationSeconds);
    public int PhotoSeconds => Math.Max(1, Settings.PhotoRotationSeconds);

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
            OnPropertyChanged(nameof(TodaySun));
            OnPropertyChanged(nameof(SunriseDisplay));
            OnPropertyChanged(nameof(SunsetDisplay));
            OnPropertyChanged(nameof(DisplayWeather));
            OnPropertyChanged(nameof(DisplayTides));
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
            if (Settings.ShowDetailedWeatherMode && DisplayWeather.Any())
            {
                SetMode(DisplayMode.Weather, WeatherSeconds);
            }
            else if (ActiveAnnouncements.Any())
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

        if (_mode == DisplayMode.Weather)
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
        OnPropertyChanged(nameof(IsWeatherVisible));
        OnPropertyChanged(nameof(IsAnnouncementVisible));
        OnPropertyChanged(nameof(IsPhotoVisible));
        OnPropertyChanged(nameof(IsPhotoFallbackVisible));
        OnPropertyChanged(nameof(HasCurrentPhotoText));
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
        BackgroundImagePath = data.BackgroundImagePath;

        Weather.Clear();
        foreach (var item in data.Weather)
        {
            Weather.Add(item);
        }

        SunEntries.Clear();
        foreach (var sun in data.SunEntries)
        {
            SunEntries.Add(sun);
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
        OnPropertyChanged(nameof(TodaySun));
        OnPropertyChanged(nameof(SunriseDisplay));
        OnPropertyChanged(nameof(SunsetDisplay));
        OnPropertyChanged(nameof(BackgroundImagePath));
        OnPropertyChanged(nameof(FilteredSchedule));
        OnPropertyChanged(nameof(HighTideDisplay));
        OnPropertyChanged(nameof(LowTideDisplay));
        OnPropertyChanged(nameof(ActivePhotos));
        OnPropertyChanged(nameof(CurrentPhoto));
        OnPropertyChanged(nameof(IsPhotoFallbackVisible));
        OnPropertyChanged(nameof(HasCurrentPhotoText));
        OnPropertyChanged(nameof(DisplayWeather));
        OnPropertyChanged(nameof(IsWeatherVisible));
    }


    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


    public void RestartRotation()
    {
        _announcementIndex = 0;
        _photoIndex = 0;

        SetMode(DisplayMode.Schedule, ScheduleSeconds);
    }
}
