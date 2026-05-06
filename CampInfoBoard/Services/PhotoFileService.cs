using System.IO;

namespace CampInfoBoard.Services;

public static class PhotoFileService
{
    public static string ImportPhoto(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return "";
        }

        string resolvedSourcePath = AppPaths.ResolveBoardPath(sourcePath);

        if (!File.Exists(resolvedSourcePath))
        {
            return sourcePath;
        }

        AppPaths.EnsureFolders();

        string sourceFullPath = Path.GetFullPath(resolvedSourcePath);
        string photosFolder = Path.GetFullPath(AppPaths.PhotosDirectory);

        if (sourceFullPath.StartsWith(photosFolder, StringComparison.OrdinalIgnoreCase))
        {
            return AppPaths.MakeBoardRelativePath(sourceFullPath);
        }

        string fileName = Path.GetFileName(sourceFullPath);
        string destinationPath = Path.Combine(AppPaths.PhotosDirectory, fileName);

        if (File.Exists(destinationPath))
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);

            destinationPath = Path.Combine(
                AppPaths.PhotosDirectory,
                $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}");
        }

        File.Copy(sourceFullPath, destinationPath);

        return AppPaths.MakeBoardRelativePath(destinationPath);
    }
}