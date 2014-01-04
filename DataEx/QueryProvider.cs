using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEx
{
    public class QueryProvider
    {
        private static Dictionary<string, object> _items;
        private static Dictionary<string, object> Items
        {
            get { return _items ?? (_items = new Dictionary<string, object>()); }
        }

        public static EntityQueryProvider<T> GetQueryProvider<T>()
        {
            var type = typeof (T).FullName;
            if (Items.ContainsKey(type))
                return (EntityQueryProvider<T>) Items[type];
            var newItem = new EntityQueryProvider<T>();
            Items[type] = newItem;
            return newItem;
        }
    }
}
