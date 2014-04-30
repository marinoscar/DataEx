using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilEx
{
    public static class TimeZoneHelper
    {
        private static TimeZoneInfo _computerTz;
        private static TimeZoneInfo _costaRicaTz;

        public static TimeZoneInfo TimeZone
        {
            get { return _computerTz ?? (_computerTz = TimeZoneInfo.Local); }
        }

        public static TimeZoneInfo CostaRicaTimeZone
        {
            get { return _costaRicaTz ?? (_costaRicaTz = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time")); }
        }

        public static DateTime CostaRicaTime
        {
            get { return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, CostaRicaTimeZone); }
        }

        public static DateTime FromUtc(TimeZoneInfo tz, DateTime utcTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
        }

        public static DateTime FromUtcToCostaRicaTime(DateTime utcTime)
        {
            return FromUtc(CostaRicaTimeZone, utcTime);
        }
    }
}
