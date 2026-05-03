using CampInfoBoard.Models;
using System.IO;
using System.Text.Json;

namespace CampInfoBoard.Services
{
    public static class DataService
    {
        private static string DataFilePath => AppPaths.JsonFilePath;

        public static AppData LoadData()
        {
            try
            {
                AppPaths.EnsureFolders();

                if (!File.Exists(DataFilePath))
                {
                    var defaultData = CreateDefaultData();
                    SaveData(defaultData);
                    return defaultData;
                }

                string json = File.ReadAllText(DataFilePath);

                var data = JsonSerializer.Deserialize<AppData>(json);

                return data ?? CreateDefaultData();
            }
            catch
            {
                return CreateDefaultData();
            }
        }

        public static void SaveData(AppData data)
        {
            AppPaths.EnsureFolders();

            if (File.Exists(DataFilePath))
            {
                string backupPath = Path.Combine(
                    AppPaths.BackupsDirectory,
                    $"camp-info-board-{DateTime.Now:yyyyMMdd-HHmmss}.json");

                File.Copy(DataFilePath, backupPath, true);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(data, options);

            File.WriteAllText(DataFilePath, json);

            CleanupUnusedPhotoFiles(data);
            CleanupUnusedBackgroundImages(data);
        }


        private static void CleanupUnusedPhotoFiles(AppData data)
        {
            var usedPhotoPaths = data.Photos
                .Select(p => p.ImagePath)
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Select(path =>
                {
                    try
                    {
                        return Path.GetFullPath(path);
                    }
                    catch
                    {
                        return "";
                    }
                 })
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!Directory.Exists(AppPaths.PhotosDirectory))
                return;

            foreach (string filePath in Directory.GetFiles(AppPaths.PhotosDirectory))
            {
                string fullPath = Path.GetFullPath(filePath);

                if (!usedPhotoPaths.Contains(fullPath))
                {
                    try
                    {
                        File.Delete(fullPath);
                    }
                    catch
                    {
                        // Ignore cleanup failures so saving the board never crashes.
                        // The unused file can be cleaned up on a later save.
                    }
                }
            }
        }

        private static AppData CreateDefaultData()
        {
            var data = new AppData();

            data.Settings.ScheduleEventsPerPage = 4;

            return data;
        }



        private static void CleanupUnusedBackgroundImages(AppData data)
        {
            if (!Directory.Exists(AppPaths.BackgroundDirectory))
            {
                return;
            }

            string? activeBackgroundPath = string.IsNullOrWhiteSpace(data.BackgroundImagePath)
                ? null
                : Path.GetFullPath(data.BackgroundImagePath);

            foreach (string file in Directory.GetFiles(AppPaths.BackgroundDirectory))
            {
                try
                {
                    string fileName = Path.GetFileName(file);
                    if (!fileName.StartsWith("background-", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string fullPath = Path.GetFullPath(file);
                    if (activeBackgroundPath != null &&
                        string.Equals(fullPath, activeBackgroundPath, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    File.Delete(fullPath);
                }
                catch
                {
                    // Ignore cleanup failures so save still succeeds
                }
            }
        }

    }
}