using System.IO;

public static class AppPaths
{
    public static string LibraryDirectory { get; set; } =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "CampInfoBoard");

    public static string CurrentBoardName { get; set; } = "Default Board";

    public static string BoardsDirectory =>
        Path.Combine(LibraryDirectory, "Boards");

    public static string CurrentBoardDirectory =>
        Path.Combine(BoardsDirectory, CurrentBoardName);

    public static string DataDirectory =>
        Path.Combine(CurrentBoardDirectory, "Data");

    public static string PhotosDirectory =>
        Path.Combine(CurrentBoardDirectory, "Photos");

    public static string JsonFilePath =>
        Path.Combine(DataDirectory, "camp-info-board.json");

    public static string BackupsDirectory =>
        Path.Combine(CurrentBoardDirectory, "Backups");

    public static void EnsureFolders()
    {
        Directory.CreateDirectory(DataDirectory);
        Directory.CreateDirectory(PhotosDirectory);
        Directory.CreateDirectory(BackupsDirectory);
    }


    public static void SaveBoardAs(string newBoardName)
    {
        string safeName = SanitizeBoardName(newBoardName);

        if (string.IsNullOrWhiteSpace(safeName))
            throw new ArgumentException("Board name cannot be empty.");

        string sourceDirectory = CurrentBoardDirectory;
        string oldBoardName = CurrentBoardName;

        CurrentBoardName = safeName;
        string targetDirectory = CurrentBoardDirectory;

        CurrentBoardName = oldBoardName;

        if (Directory.Exists(targetDirectory))
            throw new IOException("A board with that name already exists.");

        EnsureFolders();

        CopyDirectory(sourceDirectory, targetDirectory);

        CurrentBoardName = safeName;
        EnsureFolders();
    }

    private static string SanitizeBoardName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }

        return name.Trim();
    }

    private static void CopyDirectory(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);

        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string targetFile = Path.Combine(targetDir, Path.GetFileName(file));
            File.Copy(file, targetFile, overwrite: false);
        }

        foreach (string directory in Directory.GetDirectories(sourceDir))
        {
            string targetSubDir = Path.Combine(targetDir, Path.GetFileName(directory));
            CopyDirectory(directory, targetSubDir);
        }
    }


    public static IEnumerable<string> GetAvailableBoards()
    {
        if (!Directory.Exists(BoardsDirectory))
        {
            return Enumerable.Empty<string>();
        }

        return Directory
            .GetDirectories(BoardsDirectory)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .OrderBy(name => name)!;
    }
}