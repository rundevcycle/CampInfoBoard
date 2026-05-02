using CampInfoBoard.Models;
using System.Globalization;
using System.IO;

namespace CampInfoBoard.Services
{
    public static class SunImportService
    {
        public static SunImportResult ImportFromCsv(string filePath)
        {
            var imported = new List<SunEntry>();
            int skipped = 0;

            foreach (string line in File.ReadLines(filePath).Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] parts = line.Split(',');

                if (parts.Length < 3)
                {
                    skipped++;
                    continue;
                }

                if (!DateTime.TryParse(parts[0].Trim(), out DateTime date))
                {
                    skipped++;
                    continue;
                }

                if (!TryParseTime(parts[1].Trim(), out TimeSpan sunrise))
                {
                    skipped++;
                    continue;
                }

                if (!TryParseTime(parts[2].Trim(), out TimeSpan sunset))
                {
                    skipped++;
                    continue;
                }

                imported.Add(new SunEntry
                {
                    Date = date.Date,
                    Sunrise = date.Date.Add(sunrise),
                    Sunset = date.Date.Add(sunset)
                });
            }

            return new SunImportResult(imported, skipped);
        }

        private static bool TryParseTime(string value, out TimeSpan time)
        {
            time = default;

            if (DateTime.TryParse(
                    value,
                    CultureInfo.CurrentCulture,
                    DateTimeStyles.None,
                    out DateTime parsedTime))
            {
                time = parsedTime.TimeOfDay;
                return true;
            }

            string digits = new(value.Where(char.IsDigit).ToArray());

            if (digits.Length is < 1 or > 4)
            {
                return false;
            }

            int hour;
            int minute;

            if (digits.Length <= 2)
            {
                hour = int.Parse(digits);
                minute = 0;
            }
            else
            {
                hour = int.Parse(digits[..^2]);
                minute = int.Parse(digits[^2..]);
            }

            if (hour is < 0 or > 23 || minute is < 0 or > 59)
            {
                return false;
            }

            time = new TimeSpan(hour, minute, 0);
            return true;
        }
    }

    public record SunImportResult(
        List<SunEntry> ImportedSunEntries,
        int SkippedRows);
}