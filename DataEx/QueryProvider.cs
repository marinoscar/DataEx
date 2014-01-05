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

        public static IStandardEntityQueryProvider GetQueryProvider(Type type)
        {
            var typeName = type.FullName;
            if (!Items.ContainsKey(typeName))
            {
                Items[typeName] = CreateProvider(type);
            }
            return (IStandardEntityQueryProvider)Items[typeName];
        }

        public static IStandardEntityQueryProvider CreateProvider(Type type)
        {
            var generic = typeof (EntityQueryProvider<>);
            var genericType = generic.MakeGenericType(type);
            return (IStandardEntityQueryProvider)Activator.CreateInstance(genericType);
        }
    }
}
