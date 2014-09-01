using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilEx
{
    public static class ULongExtension
    {
        public static void Times(this ulong i, Action<ulong> action)
        {
            for (ulong j = 0; j < i; j++)
            {
                action(j);
            }
        }

        public static string ToBinary(this ulong i)
        {
            var numBaseConv = new NumericBaseConverter();
            return numBaseConv.ToBinary(i);
        }

        public static string ToHex(this ulong i)
        {
            var numBaseConv = new NumericBaseConverter();
            return numBaseConv.ToHex(i);
        }

        public static string ToBase36(this ulong i)
        {
            var numBaseConv = new NumericBaseConverter();
            return numBaseConv.ToBase36(i);
        }

        public static string ToBase(this ulong i, int numericBase)
        {
            var numBaseConv = new NumericBaseConverter();
            return numBaseConv.ToBase(i, numericBase);
        }
    }
}
