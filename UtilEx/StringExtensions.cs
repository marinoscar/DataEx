/*

Copyright (C) 2007-2011 by Gustavo Duarte and Bernardo Vieira.
 
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
 
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UtilEx
{
    public static class StringExtensions
    {
        public static readonly Char[] MagicRegexChars = new[] { '^', '$', '.', '(', ')', '{', '\\', '[', '?', '+', '*', '|' };
        public static readonly Char[] MagicSqlLikeChars = new[] { '%', '_', '[', ']' };
        public static readonly Char[] AngleBrackets = new[] { '<', '>' };
        public static readonly Regex EmailPattern = new Regex(@"^[\w._%+-]+@[\w.-]+\.[A-Za-z]{2,4}$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary>
        /// Formats a string and arguments using the <see cref="CultureInfo.InvariantCulture">invariant culture</see>.
        /// </summary>
        /// <remarks>
        /// <para>This should <b>not</b> be used for any strings that are displayed to the user. It is meant for log
        /// messages, exception messages, and other types of information that do not make it into the UI, or wouldn't
        /// make sense to users anyway ;).</para>
        /// </remarks>
        public static string FormatInvariant(this string format, params object[] args)
        {
            return 0 == args.Length ? format : string.Format(CultureInfo.InvariantCulture, format, args);
        }

        public static string FormatSql(this string format, params object[] args)
        {
            return string.Format(SqlFormatter.Instance, format, args);
        }
        /// <summary>
        /// Alias for <see cref="FormatInvariant" />.
        /// </summary>
        public static string Fi(this string format, params object[] args)
        {
            return FormatInvariant(format, args);
        }

        public static string EscapeMagicSqlLikeChars(this string s)
        {
            // make the common case fast
            if (-1 == s.LastIndexOfAny(MagicSqlLikeChars))
            {
                return s;
            }

            for (var i = 0; i < MagicSqlLikeChars.Length; i++)
            {
                var c = MagicSqlLikeChars[i];
                if (s.LastIndexOf(c) != -1)
                {
                    s = s.Replace(c.ToString(), "[" + c + "]");
                }
            }

            return s;
        }

        /// <summary>
        /// Splits the string into an string array of different sizes
        /// </summary>
        /// <param name="s">string to work on</param>
        /// <param name="itemSize">The size of the new string</param>
        /// <returns>IEnumerable with the new string</returns>
        public static IEnumerable<string> Split(this string s, int itemSize)
        {
            if (s.Length <= itemSize) return new [] { s };
            return Enumerable.Range(0, s.Length / itemSize).Select(i => s.Substring(i * itemSize, itemSize));
        }



    }
}
