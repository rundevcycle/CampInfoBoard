namespace CampInfoBoard.Models
{
    public class AppData
    {
        public DisplaySettings Settings { get; set; } = new();

        public List<WeatherBlock> Weather { get; set; } = new();

        public List<TideEntry> TideEntries { get; set; } = new();

        public List<SunEntry> SunEntries { get; set; } = new();

        public RadioInfo Radio { get; set; } = new();

        public List<ScheduleItem> Schedule { get; set; } = new();

        public List<Announcement> Announcements { get; set; } = new();

        public List<PhotoItem> Photos { get; set; } = new();

        public string BackgroundImagePath { get; set; } = "";
    }
}