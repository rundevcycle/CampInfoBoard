namespace CampInfoBoard.Models
{
    public class DisplaySettings
    {
        public bool ShowWeather { get; set; } = true;
        public bool ShowSun { get; set; } = true;
        public bool ShowTides { get; set; } = true;
        public int DisplayMonitorIndex { get; set; } = 0;
        public MeasurementDisplayMode MeasurementMode { get; set; } =
            MeasurementDisplayMode.Both;
        public int ScheduleRotationSeconds { get; set; } = 30;
        public int ScheduleEventsPerPage { get; set; } = 4;
        public int ScheduleLookAheadDays { get; set; } = 1;
        public int WeatherRotationSeconds { get; set; } = 15;
        public bool ShowDetailedWeatherMode { get; set; } = true;
        public int AnnouncementRotationSeconds { get; set; } = 10;
        public int PhotoRotationSeconds { get; set; } = 15;
        public bool DisplayAlwaysOnTop { get; set; } = false;

        public string BackgroundColor { get; set; } = "#101820";

        public bool ShowBanner { get; set; }

        public string BannerText { get; set; } = "";

        public BannerPosition BannerPosition { get; set; } = BannerPosition.Top;

        public double DisplayFontScale { get; set; } = 1.0;
    }
}
