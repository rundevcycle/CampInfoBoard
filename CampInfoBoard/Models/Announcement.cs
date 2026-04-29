namespace CampInfoBoard.Models
{
    public class Announcement
    {
        public string Title { get; set; } = "";
        public string BodyText { get; set; } = "";
        public string? ImagePath { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public int Priority { get; set; }
    }
}
