using System.IO;

namespace CampInfoBoard.Services;

public static class PhotoFileService
{
    public static string ImportPhoto(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
        {
            return sourcePath;
        }

        AppPaths.EnsureFolders();

        string fileName = Path.GetFileName(sourcePath);
        string destinationPath = Path.Combine(AppPaths.PhotosDirectory, fileName);

        if (Path.GetFullPath(sourcePath).Equals(Path.GetFullPath(destinationPath), StringComparison.OrdinalIgnoreCase))
        {
            return sourcePath;
        }

        if (File.Exists(destinationPath))
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);

            destinationPath = Path.Combine(
                AppPaths.PhotosDirectory,
                $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}");
        }

        File.Copy(sourcePath, destinationPath);

        return destinationPath;
    }
}