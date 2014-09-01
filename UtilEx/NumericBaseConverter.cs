using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilEx
{
    public class NumericBaseConverter
    {
        private readonly List<string> _baseValues = new List<string>() { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

        public string ToBase(ulong value, int numericBase)
        {
            ValidateBase(numericBase);
            var components = new List<string>();
            var quotient = (double)value;
            while (quotient > 0)
            {
                var remainder = (int)Math.Floor(quotient % numericBase);
                quotient = Math.Floor(quotient / numericBase);
                components.Add(_baseValues[remainder]);
            }
            components.Reverse();
            return string.Join(string.Empty, components).ToLowerInvariant();
        }

        public ulong FromBase(string value, int numericBase)
        {
            ValidateBase(numericBase);
            var values = value.ToUpperInvariant().ToCharArray().Select(i => _baseValues.IndexOf(i.ToString(CultureInfo.InvariantCulture))).ToArray();
            var valueCount = values.Count();
            double result = 0;
            for (var i = 0; i < valueCount; i++)
            {
                var powResult = Math.Pow(numericBase, ((valueCount - i)-1));
                result = result + (powResult * values[i]);
            }
            return Convert.ToUInt64(result);
        }

        private void ValidateBase(int numericBase)
        {
            if (numericBase < 2 || numericBase > _baseValues.Count())
                throw new ArgumentException("The numeric base needs to be between 2 and {0}".Fi(_baseValues.Count()));
        }

        public string ToBinary(ulong value)
        {
            return ToBase(value, 2);
        }

        public string ToHex(ulong value)
        {
            return ToBase(value, 16);
        }

        public string ToBase36(ulong value)
        {
            return ToBase(value, 36);
        }
    }
}
