using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilEx
{
    public static class IntExtension
    {
        public static void Times(this int i, Action<int> action)
        {
            for (var j = 0; j < i; j++)
            {
                action(j);
            }
        }

        public static string ToBinary(this int i)
        {
            var numBaseConv = new NumericBaseConverter();
            return numBaseConv.ToBinary(Convert.ToUInt64(i));
        }

        public static string ToHex(this int i)
        {
            var numBaseConv = new NumericBaseConverter();
            return numBaseConv.ToHex(Convert.ToUInt64(i));
        }

        public static string ToBase36(this int i)
        {
            var numBaseConv = new NumericBaseConverter();
            return numBaseConv.ToBase36(Convert.ToUInt64(i));
        }

        public static string ToBase(this int i, int numericBase)
        {
            var numBaseConv = new NumericBaseConverter();
            return numBaseConv.ToBase(Convert.ToUInt64(i), numericBase);
        }
    }
}
