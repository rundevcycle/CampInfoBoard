using System.Globalization;

namespace CampInfoBoard.Models
{
    public class TideEntry
    {
        public DateTime Time { get; set; } = DateTime.Today;
        public TideType TideLevel { get; set; }


        public DateTime Date
        {
            get => Time.Date;
            set
            {
                Time = value.Date + Time.TimeOfDay;
            }
        }


        public string DateText
        {
            get => Date.ToString("d", CultureInfo.CurrentCulture);
            set
            {
                if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsed))
                {
                    Date = parsed;
                }
            }
        }


        public string TimeText
        {
            get => Time.ToString("h:mm tt");
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                string input = value.Trim().ToLower();

                bool isPm = input.Contains("p");
                bool isAm = input.Contains("a");

                input = input
                    .Replace("am", "")
                    .Replace("pm", "")
                    .Replace("a", "")
                    .Replace("p", "")
                    .Replace(":", "")
                    .Trim();

                int hours;
                int minutes;

                if (input.Length <= 2)
                {
                    if (!int.TryParse(input, out hours))
                    {
                        return;
                    }

                    minutes = 0;
                }
                else
                {
                    string hourPart = input.Substring(0, input.Length - 2);
                    string minutePart = input.Substring(input.Length - 2);

                    if (!int.TryParse(hourPart, out hours))
                    {
                        return;
                    }

                    if (!int.TryParse(minutePart, out minutes))
                    {
                        return;
                    }
                }

                if (minutes < 0 || minutes > 59)
                {
                    return;
                }

                if (isPm && hours < 12)
                {
                    hours += 12;
                }
                else if (isAm && hours == 12)
                {
                    hours = 0;
                }
                else if (!isPm && !isAm)
                {
                    // Default assumption:
                    // 1–6 = AM
                    // 7–11 = AM
                    // 12 = PM
                    if (hours == 12)
                    {
                        hours = 12;
                    }
                    else if (hours >= 1 && hours <= 11)
                    {
                        // Keep as AM by default
                    }
                }

                if (hours < 0 || hours > 23)
                {
                    return;
                }

                Time = Time.Date + new TimeSpan(hours, minutes, 0);
            }
        }
    }
}
