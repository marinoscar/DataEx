using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilEx
{
    public static class DateTimeExtension
    {
        private static readonly DateTime EpochStart = new DateTime(1970, 1, 1);

        public static uint ToEpoch(this DateTime d)
        {
            return (uint) d.Subtract(EpochStart).TotalMilliseconds;
        }

        public static uint ToInt(this DateTime d)
        {
            return ToEpoch(d);
        }
    }
}
