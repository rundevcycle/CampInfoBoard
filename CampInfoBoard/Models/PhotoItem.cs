namespace CampInfoBoard.Models
{
    public class PhotoItem
    {
        public string ImagePath { get; set; } = "";
        public string Caption { get; set; } = "";
        public string Credit { get; set; } = "";
        public DateTime Added { get; set; } = DateTime.Now;
        public TimeSpan Duration { get; set; } = TimeSpan.FromHours(24);
    }
}
