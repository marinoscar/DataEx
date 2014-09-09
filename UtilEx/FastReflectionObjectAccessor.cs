using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fasterflect;
using System.Reflection;

namespace UtilEx
{
    public class FastReflectionObjectAccessor : IObjectAccesor
    {

        private const string ReflectionMemberGetterCacheProvider = "ReflectionMemberGetterCacheProvider";
        private const string ReflectionMemberSetterCacheProvider = "ReflectionMemberSetterCacheProvider";
        private const string ReflectionConstrutorInvokerCacheProvider = "ReflectionConstrutorInvokerCacheProvider";

        public object Create(Type type)
        {
            var provider =
                ObjectCacheProvider.GetProvider<Type, ConstructorInvoker>(ReflectionConstrutorInvokerCacheProvider);
            var constructor = provider.GetCacheItem(type, i => i.DelegateForCreateInstance(null));
            return constructor(null);
        }

        public T Create<T>()
        {
            return (T)Create(typeof (T));
        }

        public object GetPropertyValue(object target, string propertyName)
        {
            var key = new Tuple<Type, string>(target.GetType(), propertyName);
            var provider = ObjectCacheProvider.GetProvider<Tuple<Type, string>, MemberGetter>(ReflectionMemberGetterCacheProvider);
            var value = provider.GetCacheItem(key, i => i.Item1.DelegateForGetPropertyValue(i.Item2));
            return value(target);
        }

        public T GetPropertyValue<T>(object target, string propertyName)
        {
            return (T)Convert.ChangeType(GetPropertyValue(target, propertyName), typeof(T));
        }

        public T TryGetPropertyValue<T>(object target, string propertyName)
        {
            var result = default(T);
            try
            {
                result = GetPropertyValue<T>(target, propertyName);
            }
            finally
            {
            }
            return result;
        }

        public void SetPropertyValue(object target, string propertyName, object value)
        {
            var key = new Tuple<Type, string>(target.GetType(), propertyName);
            var provider = ObjectCacheProvider.GetProvider<Tuple<Type, string>, MemberSetter>(ReflectionMemberSetterCacheProvider);
            var valueMethod = provider.GetCacheItem(key, i => i.Item1.DelegateForSetPropertyValue(i.Item2));
            valueMethod(target, value);
        }

        public void TrySetPropertyValue(object target, string propertyName, object value)
        {
            try
            {
                SetPropertyValue(target,propertyName,value);
            }
            finally
            { }
        }
    }
}
