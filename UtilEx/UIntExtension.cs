using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilEx
{
    public static class UIntExtension
    {
        public static void Times(this uint i, Action<int> action)
        {
            for (var j = 0; j < i; j++)
            {
                action(j);
            }
        }

        public static string ToBinary(this uint i)
        {
            var numBaseConv = new NumericBaseConverter();
            return numBaseConv.ToBinary(i);
        }

        public static string ToHex(this uint i)
        {
            var numBaseConv = new NumericBaseConverter();
            return numBaseConv.ToHex(i);
        }

        public static string ToBase36(this uint i)
        {
            var numBaseConv = new NumericBaseConverter();
            return numBaseConv.ToBase36(i);
        }

        public static string ToBase(this uint i, int numericBase)
        {
            var numBaseConv = new NumericBaseConverter();
            return numBaseConv.ToBase(i, numericBase);
        }
    }
}
