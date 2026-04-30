using CampInfoBoard.Models;
using System.Collections.ObjectModel;

namespace CampInfoBoard.ViewModels;

public class PhotoManagerViewModel
{
    public ObservableCollection<PhotoItem> Photos { get; } = new();

    public void Load(IEnumerable<PhotoItem> items)
    {
        Photos.Clear();

        foreach (var item in items.OrderBy(p => p.DisplayOrder))
            Photos.Add(item);
    }

    public List<PhotoItem> ToList()
    {
        return Photos
            .Select((p, index) =>
            {
                p.DisplayOrder = index;
                return p;
            })
            .ToList();
    }

    public PhotoItem Copy(PhotoItem source)
    {
        var copy = new PhotoItem
        {
            ImagePath = source.ImagePath,
            Caption = source.Caption,
            Credit = source.Credit,
            ExpiryDate = source.ExpiryDate,
            IsActive = source.IsActive,
            DisplayOrder = Photos.Count
        };

        Photos.Add(copy);
        return copy;
    }

    public void Delete(PhotoItem item)
    {
        Photos.Remove(item);
    }
}