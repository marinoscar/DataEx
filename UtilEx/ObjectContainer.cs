using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilEx
{
    public static class ObjectContainer
    {
        private const string CacheProviderKey = "ObjectContainerCacheKey";

        private static CacheItems<Type, ContainerItem> GetCache()
        {
            return ObjectCacheProvider.GetProvider<Type, ContainerItem>(CacheProviderKey);
        }

        /// <summary>
        /// Register as object. It will be created everytime is called
        /// </summary>
        /// <typeparam name="T">Object Type to resolve</typeparam>
        /// <param name="instance">The constructor for the item</param>
        public static void Register<T>(Func<object> instance)
        {
            var cache = GetCache();
            cache.SetCacheItem(typeof(T), new ContainerItem()
                {
                    Constructor = instance,
                    Item = null
                });

        }

        /// <summary>
        /// Register the object as a singleton item
        /// </summary>
        /// <typeparam name="T">Object Type to resolve</typeparam>
        /// <param name="item"></param>
        public static void Register<T>(object item)
        {
            var cache = GetCache();
            cache.SetCacheItem(typeof(T), new ContainerItem()
            {
                Constructor = null,
                Item = item
            });
        }

        public static bool IsRegistered<T>()
        {
            var cache = GetCache();
            return cache.ContainsKey(typeof (T));
        }

        public static T Get<T>()
        {
            var cache = GetCache();
            var item = cache.GetCacheItem(typeof(T));
            var result = item.Constructor == null ? item.Item : item.Constructor();
            return (T)result;
        }

        private class ContainerItem
        {
            public object Item { get; set; }
            public Func<object> Constructor { get; set; }
        }
    }
}
