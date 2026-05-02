using CampInfoBoard.Models;
using System.Globalization;
using System.IO;

namespace CampInfoBoard.Services
{
    public static class TideImportService
    {

        /// <summary>
        /// Import tides from a CSV file.  The file has a header row that will be ignored.
        /// 1. Date
        /// 2. Time
        /// 3. High or Low
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static TideImportResult ImportFromCsv(string filePath)
        {
            var imported = new List<TideEntry>();
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

                string dateText = parts[0].Trim();
                string timeText = parts[1].Trim();
                string tideText = parts[2].Trim();

                if (!DateTime.TryParse(
                        $"{dateText} {timeText}",
                        CultureInfo.CurrentCulture,
                        DateTimeStyles.None,
                        out DateTime tideTime))
                {
                    skipped++;
                    continue;
                }

                if (!Enum.TryParse(tideText, ignoreCase: true, out TideType tideLevel))
                {
                    skipped++;
                    continue;
                }

                imported.Add(new TideEntry
                {
                    Time = tideTime,
                    TideLevel = tideLevel
                });
            }

            return new TideImportResult(imported, skipped);
        }
    }

    public record TideImportResult(
        List<TideEntry> ImportedTides,
        int SkippedRows);
}