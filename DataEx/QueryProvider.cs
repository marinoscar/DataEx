using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilEx;

namespace DataEx
{
    public class QueryProvider
    {
        private const string CacheProviderKey = "entityQueryProvider";

        public static EntityQueryProvider<T> GetQueryProvider<T>()
        {
            var type = typeof (T);
            var cacheProvider = ObjectCacheProvider.GetProvider<Type, IEntityQueryProvider>(CacheProviderKey);
            return (EntityQueryProvider<T>)cacheProvider.GetCacheItem(type, i => new EntityQueryProvider<T>());
        }

        public static IEntityQueryProvider GetQueryProvider(Type type)
        {
            var cacheProvider = ObjectCacheProvider.GetProvider<Type, IEntityQueryProvider>(CacheProviderKey);
            return cacheProvider.GetCacheItem(type, i => CreateProvider(type));
        }

        public static IEntityQueryProvider CreateProvider(Type type)
        {
            var generic = typeof (EntityQueryProvider<>);
            var genericType = generic.MakeGenericType(type);
            return (IEntityQueryProvider)Activator.CreateInstance(genericType);
        }
    }
}
