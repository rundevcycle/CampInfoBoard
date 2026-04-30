using System.Globalization;

namespace CampInfoBoard.Models
{
    public class ScheduleItem
    {
        public DateTime Start { get; set; } = DateTime.Today;
        public DateTime End { get; set; } = DateTime.Today.AddHours(1);
        public string Title { get; set; } = "";
        public string Location { get; set; } = "";
        public string Speaker { get; set; } = "";
        public string Description { get; set; } = "";

        public string TimeRange => $"{Start:h:mm tt} – {End:h:mm tt}";


        public DateTime StartDate
        {
            get => Start.Date;
            set
            {
                Start = value.Date + Start.TimeOfDay;
            }
        }

        public string StartDateText
        {
            get => StartDate.ToString("d", CultureInfo.CurrentCulture);
            set
            {
                if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsed))
                {
                    StartDate = parsed;
                }
            }
        }

        public string StartTimeText
        {
            get => Start.ToString("h:mm tt");
            set
            {
                if (TryParseFlexibleTime(value, out TimeSpan time))
                {
                    Start = Start.Date + time;

                    if (End <= Start)
                    {
                        End = Start.AddHours(1);
                    }
                }
            }
        }

        public DateTime EndDate
        {
            get => End.Date;
            set
            {
                End = value.Date + End.TimeOfDay;
            }
        }

        public string EndDateText
        {
            get => EndDate.ToString("d", CultureInfo.CurrentCulture);
            set
            {
                if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsed))
                {
                    EndDate = parsed;
                }
            }
        }

        public string EndTimeText
        {
            get => End.ToString("h:mm tt");
            set
            {
                if (TryParseFlexibleTime(value, out TimeSpan time))
                {
                    End = End.Date + time;

                    if (End <= Start)
                    {
                        End = Start.AddHours(1);
                    }
                }
            }
        }

        private static bool TryParseFlexibleTime(string value, out TimeSpan time)
        {
            time = TimeSpan.Zero;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
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
                    return false;
                }

                minutes = 0;
            }
            else
            {
                string hourPart = input.Substring(0, input.Length - 2);
                string minutePart = input.Substring(input.Length - 2);

                if (!int.TryParse(hourPart, out hours))
                {
                    return false;
                }

                if (!int.TryParse(minutePart, out minutes))
                {
                    return false;
                }
            }

            if (minutes < 0 || minutes > 59)
            {
                return false;
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
                if (hours == 12)
                {
                    hours = 12;
                }
            }

            if (hours < 0 || hours > 23)
            {
                return false;
            }

            time = new TimeSpan(hours, minutes, 0);
            return true;
        }
    }
}
