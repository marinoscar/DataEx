using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEx
{
    public static class IDataRecordExtensions
    {
        public static int GetOrdinalOrMinusOne(this IDataRecord r, string columnName)
        {
            var cntFields = r.FieldCount;
            for (var i = 0; i < cntFields; i++)
            {
                if (r.GetName(i) == columnName)
                {
                    return i;
                }
            }

            return -1;
        }

        public static bool HasColumn(this IDataRecord r, string columnName)
        {
            return r.GetOrdinalOrMinusOne(columnName) != -1;
        }

        public static T TryGet<T>(this IDataRecord r, string columnName)
        {
            var idx = r.GetOrdinalOrMinusOne(columnName);
            if (-1 == idx)
            {
                return default(T);
            }

            var v = r[idx];
            return Convert.IsDBNull(v) ? default(T) : (T)v;
        }

        public static Dictionary<string, object> ToDictionary(this IDataRecord r)
        {
            var result = new Dictionary<string, object>();
            for (var i = 0; i < r.FieldCount; i++)
            {
                result.Add(r.GetName(i), r.GetValue(i));
            }
            return result;
        }
    }
}
