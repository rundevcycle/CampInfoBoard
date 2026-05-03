namespace CampInfoBoard.Models
{
    public class SunEntry
    {
        public DateTime Date { get; set; } = DateTime.Today;

        public DateTime Sunrise { get; set; } = DateTime.Today;

        public DateTime Sunset { get; set; } = DateTime.Today;

        public bool IsPast => Date.Date < DateTime.Today;

        public string DateText
        {
            get => Date.ToShortDateString();
            set
            {
                if (DateTime.TryParse(value, out DateTime result))
                {
                    Date = result.Date;
                    Sunrise = Date.Date.Add(Sunrise.TimeOfDay);
                    Sunset = Date.Date.Add(Sunset.TimeOfDay);
                }
            }
        }

        public string SunriseText
        {
            get => Sunrise.ToShortTimeString();
            set
            {
                if (TryParseTimeText(value, out TimeSpan time))
                {
                    Sunrise = Date.Date.Add(time);
                }
            }
        }

        public string SunsetText
        {
            get => Sunset.ToShortTimeString();
            set
            {
                if (TryParseTimeText(value, out TimeSpan time))
                {
                    Sunset = Date.Date.Add(time);
                }
            }
        }

        private static bool TryParseTimeText(string value, out TimeSpan time)
        {
            time = default;

            if (DateTime.TryParse(value, out DateTime parsedTime))
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
}