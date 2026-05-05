using CampInfoBoard.Models;
using System.IO;
using System.Text.Json;

namespace CampInfoBoard.Services
{
    public static class AppPreferencesService
    {
        public static AppPreferences Load()
        {
            try
            {
                if (!File.Exists(AppPaths.PreferencesFilePath))
                {
                    return new AppPreferences();
                }

                string json = File.ReadAllText(AppPaths.PreferencesFilePath);

                return JsonSerializer.Deserialize<AppPreferences>(json)
                    ?? new AppPreferences();
            }
            catch
            {
                return new AppPreferences();
            }
        }

        public static void Save(AppPreferences preferences)
        {
            Directory.CreateDirectory(AppPaths.LibraryDirectory);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(preferences, options);

            File.WriteAllText(AppPaths.PreferencesFilePath, json);
        }

        public static void SaveLastBoardName(string boardName)
        {
            Save(new AppPreferences
            {
                LastBoardName = boardName
            });
        }
    }
}