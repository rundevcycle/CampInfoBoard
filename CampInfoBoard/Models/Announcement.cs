using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace CampInfoBoard.Models
{
    public class Announcement : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Title { get; set; } = "";
        public string BodyText { get; set; } = "";
        public string? ImagePath { get; set; }

        public DateTime? Start { get; set; } = DateTime.Today;
        public DateTime? End { get; set; } = DateTime.Today.AddDays(1);

        public int Priority { get; set; }

        public string StartDateText
        {
            get => Start?.ToString("d", CultureInfo.CurrentCulture) ?? "";
            set
            {
                if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsed))
                {
                    DateTime current = Start ?? DateTime.Today;
                    Start = parsed.Date + current.TimeOfDay;
                    OnPropertyChanged(nameof(Start));
                    OnPropertyChanged();
                }
            }
        }

        public string StartTimeText
        {
            get => Start?.ToString("h:mm tt") ?? "";
            set
            {
                if (TryParseFlexibleTime(value, out TimeSpan time))
                {
                    DateTime current = Start ?? DateTime.Today;
                    Start = current.Date + time;
                    OnPropertyChanged(nameof(Start));
                    OnPropertyChanged();
                }
            }
        }

        public string EndDateText
        {
            get => End?.ToString("d", CultureInfo.CurrentCulture) ?? "";
            set
            {
                if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsed))
                {
                    DateTime current = End ?? DateTime.Today;
                    End = parsed.Date + current.TimeOfDay;
                    OnPropertyChanged(nameof(End));
                    OnPropertyChanged();
                }
            }
        }

        public string EndTimeText
        {
            get => End?.ToString("h:mm tt") ?? "";
            set
            {
                if (TryParseFlexibleTime(value, out TimeSpan time))
                {
                    DateTime current = End ?? DateTime.Today;
                    End = current.Date + time;
                    OnPropertyChanged(nameof(End));
                    OnPropertyChanged();
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

            if (hours < 0 || hours > 23)
            {
                return false;
            }

            time = new TimeSpan(hours, minutes, 0);
            return true;
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}