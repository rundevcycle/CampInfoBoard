namespace CampInfoBoard.Models
{
    public class DisplaySettings
    {
        public bool ShowTides { get; set; } = true;
        public bool ShowSun { get; set; } = true;
        public int DisplayMonitorIndex { get; set; } = 0;
        public MeasurementDisplayMode MeasurementMode { get; set; } =
            MeasurementDisplayMode.Both;
        public int ScheduleRotationSeconds { get; set; } = 30;
        public int WeatherRotationSeconds { get; set; } = 15;
        public bool ShowDetailedWeatherMode { get; set; } = true;
        public int AnnouncementRotationSeconds { get; set; } = 10;
        public int PhotoRotationSeconds { get; set; } = 15;
        public bool DisplayAlwaysOnTop { get; set; } = false;
    }
}
