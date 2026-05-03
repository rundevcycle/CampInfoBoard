using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace CampInfoBoard.Models
{
    public class ScheduleItem : INotifyPropertyChanged
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
                DateTime previousStartDate = Start.Date;

                Start = value.Date + Start.TimeOfDay;

                // Only auto-shift End Date if it matched the old Start Date
                if (End.Date == previousStartDate)
                {
                    End = value.Date + End.TimeOfDay;
                }

                OnPropertyChanged(nameof(Start));
                OnPropertyChanged(nameof(End));
                OnPropertyChanged(nameof(StartTimeText));
                OnPropertyChanged(nameof(EndDateText));
                OnPropertyChanged(nameof(EndTimeText));
                OnPropertyChanged(nameof(TimeRange));
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
                    TimeSpan previousDuration = End - Start;

                    if (previousDuration <= TimeSpan.Zero)
                    {
                        previousDuration = TimeSpan.FromHours(1);
                    }

                    Start = Start.Date + time;
                    End = Start + previousDuration;

                    OnPropertyChanged(nameof(Start));
                    OnPropertyChanged(nameof(End));
                    OnPropertyChanged(nameof(StartTimeText));
                    OnPropertyChanged(nameof(EndDateText));
                    OnPropertyChanged(nameof(EndTimeText));
                    OnPropertyChanged(nameof(TimeRange));
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

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        public string DisplayDayLabel
        {
            get
            {
                DateTime today = DateTime.Today;
                int dayOffset = (Start.Date - today).Days;

                if (dayOffset == 0)
                {
                    return "Today";
                }

                if (dayOffset == 1)
                {
                    return "Tomorrow";
                }

                if (dayOffset > 1 && dayOffset <= 6)
                {
                    return Start.ToString("dddd");
                }

                return Start.ToString("MMMM d");
            }
        }

        public string DisplayStartTime => Start.ToString("h:mm tt");

        public string DisplayEndTime =>
            End == default || End <= Start
                ? ""
                : End.ToString("h:mm tt");

        public bool IsHappeningNow =>
            Start <= DateTime.Now && End >= DateTime.Now;
    }
}
