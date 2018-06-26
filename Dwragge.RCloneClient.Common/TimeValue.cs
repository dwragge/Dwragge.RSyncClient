using System;
using System.Collections.Generic;
using System.Text;

namespace Dwragge.RCloneClient.Common
{
    public struct TimeValue
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
    }
}
