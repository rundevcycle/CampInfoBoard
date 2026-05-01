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
        }

        private static AppData CreateDefaultData()
        {
            var data = new AppData();

            // WEATHER
            // TODO Remove hardcoded path
            data.Weather.Add(new WeatherBlock
            {
                TempDisplay = "22°C / 72°F",
                FeelsLikeDisplay = "Feels 26°C",
                Icon = @"C:\Users\Andy\OneDrive\Campmeeting 2025\Weather Icons\sun_and_cloud_TP.png"
            });

            // TODO Remove hardcoded path
            data.Weather.Add(new WeatherBlock
            {
                TempDisplay = "18°C / 64°F",
                FeelsLikeDisplay = "Tonight",
                Icon = @"C:\Users\Andy\OneDrive\Campmeeting 2025\Weather Icons\mostly_cloudy_overnight_TP.png"
            });

            // TODO Remove hardcoded path
            data.Weather.Add(new WeatherBlock
            {
                TempDisplay = "24°C / 75°F",
                FeelsLikeDisplay = "Tomorrow",
                Icon = @"C:\Users\Andy\OneDrive\Campmeeting 2025\Weather Icons\showers_TP.png"
            });

            // RADIO
            data.Radio.Enabled = true;
            data.Radio.EnglishFrequency = "88.5";
            data.Radio.FrenchFrequency = "89.1";

            // UV
            data.UVDisplay = "UV 6";

            // SUN
            data.Sun = new SunInfo
            {
                SunriseDisplay = "Sunrise 5:42 AM",
                SunsetDisplay = "Sunset 8:57 PM"
            };

            // TIDES
            data.TideEntries.Add(new TideEntry
            {
                Time = DateTime.Today.AddHours(8).AddMinutes(15),
                TideLevel = TideType.High
            });

            data.TideEntries.Add(new TideEntry
            {
                Time = DateTime.Today.AddHours(14).AddMinutes(30),
                TideLevel = TideType.Low
            });

            data.TideEntries.Add(new TideEntry
            {
                Time = DateTime.Today.AddHours(20).AddMinutes(45),
                TideLevel = TideType.High
            });

            // SCHEDULE
            data.Schedule.Add(new ScheduleItem
            {
                Start = DateTime.Now.AddMinutes(-20),
                End = DateTime.Now.AddMinutes(40),
                Title = "Morning Meeting",
                Location = "Main Hall",
                Speaker = "Guest Speaker",
                Description = "A time of music, teaching, and encouragement."
            });

            // ANNOUNCEMENTS
            data.Announcements.Add(new Announcement
            {
                Title = "Beach Bonfire Tonight",
                BodyText = "Join us at the beach after the evening meeting.",
                Priority = 10
            });

            // TODO Remove hardcoded path
            data.BackgroundImagePath = @"c:\Users\Andy\Documents\OpenSong\Backgrounds\Water\bottom-of-the-sea-16913.jpg";

            return data;
        }
    }
}