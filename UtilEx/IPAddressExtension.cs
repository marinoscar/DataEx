using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UtilEx
{
    public static class IpAddressExtension
    {
        public static uint ToInt(this IPAddress ip)
        {
            uint result = 0;
            var array = ip.GetAddressBytes();
            3.Times((i) =>
                {
                    result = result + (uint)(array[i] * Math.Pow(256d, Convert.ToDouble(3 - i)));
                });

            result = result + array[3];
            return result;
        }

        public static IPAddress FromInt(this IPAddress ip, uint ipAddress)
        {
            return new IPAddress(BitConverter.GetBytes(ipAddress));
        }


    }
}
