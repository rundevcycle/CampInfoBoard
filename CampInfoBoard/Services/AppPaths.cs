using System.IO;
using System.IO.Compression;

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

    public static string BackgroundDirectory =>
        Path.Combine(CurrentBoardDirectory, "Background");

    public static string AnnouncementsDirectory =>
        Path.Combine(CurrentBoardDirectory, "Announcements");

    public static string JsonFilePath =>
        Path.Combine(DataDirectory, "camp-info-board.json");

    public static string BackupsDirectory =>
        Path.Combine(CurrentBoardDirectory, "Backups");

    public static void EnsureFolders()
    {
        Directory.CreateDirectory(DataDirectory);
        Directory.CreateDirectory(PhotosDirectory);
        Directory.CreateDirectory(BackgroundDirectory);
        Directory.CreateDirectory(AnnouncementsDirectory);
        Directory.CreateDirectory(BackupsDirectory);
    }


    public static void CreateNewBoard(string newBoardName)
    {
        string safeName = SanitizeBoardName(newBoardName);

        if (string.IsNullOrWhiteSpace(safeName))
        {
            throw new ArgumentException("Board name cannot be empty.");
        }

        string oldBoardName = CurrentBoardName;

        CurrentBoardName = safeName;
        string targetDirectory = CurrentBoardDirectory;

        CurrentBoardName = oldBoardName;

        if (Directory.Exists(targetDirectory))
        {
            throw new IOException("A board with that name already exists.");
        }

        CurrentBoardName = safeName;
        EnsureFolders();
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


    public static void ExportCurrentBoardPackage(string zipPath)
    {
        EnsureFolders();

        if (File.Exists(zipPath))
        {
            File.Delete(zipPath);
        }

        using ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        ZipArchiveEntry marker = archive.CreateEntry("camp-info-board-package.txt");
        using (StreamWriter writer = new(marker.Open()))
        {
            writer.WriteLine("CampInfoBoardPackage");
            writer.WriteLine("Version=1");
        }

        AddDirectoryToZip(archive, CurrentBoardDirectory, "");
    }

    public static void ImportBoardPackage(string zipPath, string boardName)
    {
        string safeName = SanitizeBoardName(boardName);

        if (string.IsNullOrWhiteSpace(safeName))
        {
            throw new ArgumentException("Board name cannot be empty.");
        }

        using (ZipArchive archive = ZipFile.OpenRead(zipPath)) 
        {
            if (archive.GetEntry("camp-info-board-package.txt") == null)
            {
                throw new InvalidDataException("This does not appear to be a Camp Info Board package.");
            }
        }

        string targetDirectory = Path.Combine(BoardsDirectory, safeName);

        if (Directory.Exists(targetDirectory))
        {
            throw new IOException("A board with that name already exists.");
        }

        Directory.CreateDirectory(targetDirectory);

        ZipFile.ExtractToDirectory(zipPath, targetDirectory);

        CurrentBoardName = safeName;
        EnsureFolders();
    }

    private static void AddDirectoryToZip(ZipArchive archive, string sourceDirectory, string entryRoot)
    {
        foreach (string filePath in Directory.GetFiles(sourceDirectory))
        {
            string entryName = Path.Combine(entryRoot, Path.GetFileName(filePath))
                .Replace('\\', '/');

            archive.CreateEntryFromFile(filePath, entryName);
        }

        foreach (string directory in Directory.GetDirectories(sourceDirectory))
        {
            string folderName = Path.GetFileName(directory);

            if (string.Equals(folderName, "Backups", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string nextRoot = Path.Combine(entryRoot, folderName);
            AddDirectoryToZip(archive, directory, nextRoot);
        }
    }
}