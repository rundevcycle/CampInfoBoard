namespace CampInfoBoard.Models
{
    public class ScheduleItem
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Title { get; set; } = "";
        public string Location { get; set; } = "";
        public string Speaker { get; set; } = "";
        public string Description { get; set; } = "";

        public string TimeRange => $"{Start:h:mm tt} – {End:h:mm tt}";
    }
}
