using System;
using System.Collections.Generic;

namespace Dwragge.BlobBlaze.Entities
{
    public struct TimeValue : IEquatable<TimeValue>
    {
        private int _hour;
        private int _minute;
        private int _second;

        public TimeValue(int hour, int minute, int second) : this()
        {
            Hour = hour;
            Minute = minute;
            Second = Second;
        }

        public TimeValue(int hour) : this(hour, 0, 0)
        {
        }

        public TimeValue(int hour, int minute) : this(hour, minute, 0)
        {
        }

        public int Hour
        {
            get => _hour;
            set
            {
                if (value < 0 || value > 23)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Hour must be between 0 and 23 inclusive.");
                }

                _hour = value;
            }
        }

        public int Minute
        {
            get => _minute;
            set
            {
                if (value < 0 || value > 59)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Minute must be between 0 and 59 inclusive");
                }

                _minute = value;
            }
        }

        public int Second
        {
            get => _second;
            set
            {
                if (value < 0 || value > 59)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Second must be between 0 and 59 inclusive.");
                }
                _second = value;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TimeValue other))
            {
                return false;
            }

            return other.Hour == Hour
                    && other.Minute == Minute
                    && other.Second == Second;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Hour}:{Minute}:{Second}";
        }

        public static bool TryParse(string input, out TimeValue value)
        {
            value = default(TimeValue);

            if (input.Length > 8) return false;

            var splits = new List<int>(10);
            for (int i = 0; i < input.Length - 1; i++) // max length of the string could be 8: xx:xx:xx but could be shorter
            {
                if (input[i] == ':')
                {
                    splits.Add(i);
                }
            }
            if (splits.Count != 3) return false;
            if (splits[0] == 0 || splits[2] == input.Length) return false;

            var inputSpan = input.AsSpan();
            if (!int.TryParse(inputSpan.Slice(0, splits[0]), out int v1)) return false;
            if (!int.TryParse(inputSpan.Slice(splits[0] + 1, splits[1] - splits[0]), out int v2)) return false;
            if (!int.TryParse(inputSpan.Slice(splits[1] + 1, splits[2] - splits[0]), out int v3)) return false;

            value = new TimeValue(v1, v2, v3);
            return true;
        }

        public bool Equals(TimeValue other)
        {
            return other.Hour == Hour &&
                other.Minute == Minute &&
                other.Second == Second;
        }
    }
}
