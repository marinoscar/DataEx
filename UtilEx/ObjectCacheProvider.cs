using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilEx
{
    public static class ObjectCacheProvider
    {
        private static Dictionary<string, IClearableCacheItems> _cacheCollection;

        public static CacheItems<TKey, TValue> GetProvider<TKey, TValue>(string name)
        {
            CreateIfRequired();
            if (_cacheCollection.ContainsKey(name))
                return (CacheItems<TKey, TValue>)_cacheCollection[name];
            var result = new CacheItems<TKey, TValue>();
            _cacheCollection.Add(name, result);
            return result;
        }

        public static void ClearProvider(string name)
        {
            CreateIfRequired();
            if (!_cacheCollection.ContainsKey(name)) return;
            _cacheCollection[name].Clear();
        }

        public static void ClearAll()
        {
            _cacheCollection = null;
        }

        private static void CreateIfRequired()
        {
            if (_cacheCollection == null) _cacheCollection = new Dictionary<string, IClearableCacheItems>();
        }
    }

    public interface IClearableCacheItems
    {
        void Clear();
    }

    public class CacheItems<TKey, TValue> : IClearableCacheItems
    {
        public Dictionary<TKey, TValue> Internal { get; private set; }

        public CacheItems()
        {
            Internal = new Dictionary<TKey, TValue>();
        }

        public void Clear()
        {
            Internal.Clear();
        }

        /// <summary>
        /// Gets the item requested from the cache
        /// </summary>
        /// <param name="key">The key of the cache item</param>
        /// <exception cref="http://msdn.microsoft.com/en-us/library/system.argumentnullexception(v=vs.110).aspx">ArgumentNullException </exception>
        public TValue GetCacheItem(TKey key)
        {
            return GetCacheItem(key, null);
        }

        /// <summary>
        /// Gets the item requested from the cache
        /// </summary>
        /// <param name="key">The key of the cache item</param>
        /// <param name="functionToGetValue">A function to use in case the item is not in cache and requires to be stored</param>
        public TValue GetCacheItem(TKey key, Func<TKey, TValue> functionToGetValue)
        {
            if (Internal.ContainsKey(key)) return Internal[key];
            if (functionToGetValue == null)
                throw new ArgumentNullException("The key {0} is not cached and no function was provided to properly store the value in cache".Fi(key));
            var value = functionToGetValue(key);
            Internal.Add(key, value);
            return value;
        }


        //#region IDictionary Members

        //public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        //{
        //    return Internal.GetEnumerator();
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return GetEnumerator();
        //}

        //public void Add(KeyValuePair<TKey, TValue> item)
        //{
        //    Internal.Add(item.Key, item.Value);
        //}

        //public void Clear()
        //{
        //    Internal.Clear();
        //}

        //public bool Contains(KeyValuePair<TKey, TValue> item)
        //{
        //    return Internal.Contains(item);
        //}

        //public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        //{
        //    throw new NotImplementedException();
        //}

        //public bool Remove(KeyValuePair<TKey, TValue> item)
        //{
        //    Internal.Remove(item.Key);
        //}

        //public int Count
        //{
        //    get { return Internal.Count; }
        //}

        //public bool IsReadOnly
        //{
        //    get { return false; }
        //}
        //public bool ContainsKey(TKey key)
        //{
        //    return Internal.ContainsKey(key);
        //}

        //public void Add(TKey key, TValue value)
        //{
        //    Internal.Add(key, value);
        //}

        //public bool Remove(TKey key)
        //{
        //    return Internal.Remove(key);
        //}

        //public bool TryGetValue(TKey key, out TValue value)
        //{
        //    return Internal.TryGetValue(key, out value);
        //}

        //public TValue this[TKey key]
        //{
        //    get { return Internal[key]; }
        //    set { Internal[key] = value; }
        //}

        //public ICollection<TKey> Keys
        //{
        //    get { return Internal.Keys; }
        //}
        //public ICollection<TValue> Values
        //{
        //    get { return Internal.Values; }
        //}

        //#endregion

    }
}
