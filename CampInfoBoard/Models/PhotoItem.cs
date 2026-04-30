namespace CampInfoBoard.Models;

public class PhotoItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string ImagePath { get; set; } = "";
    public string Caption { get; set; } = "";
    public string Credit { get; set; } = "";

    public DateTime? ExpiryDate { get; set; }

    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    public bool IsExpired =>
        ExpiryDate.HasValue && ExpiryDate.Value.Date < DateTime.Today;
}