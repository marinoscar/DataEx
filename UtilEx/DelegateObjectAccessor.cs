using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilEx
{
    public class DelegateObjectAccessor : PropertyInfoBasedAccessor
    {

        private static Dictionary<string, Dictionary<string, Action<object, object>>> _setProperties;
        private static Dictionary<string, Dictionary<string, Func<object>>> _getProperties;
        private static Dictionary<string, Func<object>> _constructors;

        private static Dictionary<string, Dictionary<string, Action<object, object>>> SetProperties
        {
            get
            {
                return _setProperties ??
                       (_setProperties = new Dictionary<string, Dictionary<string, Action<object, object>>>());
            }
        }

        private static Dictionary<string, Dictionary<string, Func<object>>> GetProperties
        {
            get
            {
                return _getProperties ??
                     (_getProperties = new Dictionary<string, Dictionary<string, Func<object>>>());
            }
        }

        private static Dictionary<string, Func<object>> Constructors
        {
            get { return _constructors ?? (_constructors = new Dictionary<string, Func<object>>()); }
        }

        private readonly Func<object> _instanceCreator;

        public DelegateObjectAccessor()
        {
        }

        private Func<object> GetConstructor(Type type)
        {
            if (Constructors.ContainsKey(type.FullName))
                return Constructors[type.FullName];
            var constructorInfo = type.GetConstructor(new Type[] { });
            var constructor = Expression.New(constructorInfo, null);
            var newItem = Expression.Lambda(typeof(Func<object>), constructor, null);
            Constructors[type.FullName] = (Func<object>)newItem.Compile();
            return Constructors[type.FullName];
        }

        private Action<object, object> CreateSetDelegate(PropertyInfo property)
        {
            var genericMethod = GetType().GetMethod("CreateGenericSetDelegate", BindingFlags.NonPublic | BindingFlags.Static);
            var genericHelper = genericMethod.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            return (Action<object, object>)genericHelper.Invoke(null, new object[] { property.GetSetMethod() });
        }

        private static Action<object, object> CreateGenericSetDelegate<TTarget, TValue>(MethodInfo setter) where TTarget : class
        {
            var setterTypedDelegate = (Action<TTarget, TValue>)Delegate.CreateDelegate(typeof(Action<TTarget, TValue>), setter);
            var setterDelegate = (Action<object, object>)((object instance, object value) => { setterTypedDelegate((TTarget)instance, (TValue)value); });
            return setterDelegate;
        }

        public override void SetPropertyValue(object instance, string propertyName, object value)
        {
            var type = instance.GetType().FullName;
            if (!SetProperties.ContainsKey(type))
                SetProperties[type] = new Dictionary<string, Action<object, object>>();

            Action<object, object> setProp;
            if (SetProperties[type].ContainsKey(propertyName))
                setProp = SetProperties[type][propertyName];
            else
            {
                var propInfo = GetPropertyInfo(instance, propertyName);
                setProp = CreateSetDelegate(propInfo);
                SetProperties[type][propertyName] = setProp;
            }
            setProp(instance, value);
        }


        private Func<object, object> CreateGetDelegate(PropertyInfo property)
        {
            var getter = property.GetGetMethod();
            var genericMethod = GetType().GetMethod("CreateGenericGetDelegate", BindingFlags.NonPublic | BindingFlags.Static);
            var genericHelper = genericMethod.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            return (Func<object, object>)genericHelper.Invoke(null, new object[] { getter });
        }

        private static Func<object, object> CreateGenericGetDelegate<TTarget, TResult>(MethodInfo getter) where TTarget : class
        {
            var getterTypedDelegate = (Func<TTarget, TResult>)Delegate.CreateDelegate(typeof(Func<TTarget, TResult>), getter);
            var getterDelegate = (Func<object, object>)((object instance) => getterTypedDelegate((TTarget)instance));
            return getterDelegate;
        }

        public override object CreateInstance(Type type)
        {
            if (Constructors.ContainsKey(type.FullName))
                return Constructors[type.FullName]();
            Constructors[type.FullName] = GetConstructor(type);
            return Constructors[type.FullName]();
        }

        public override T CreateInstance<T>()
        {
            var result = CreateInstance(typeof (T));
            return (T)Convert.ChangeType(result, typeof (T));
        }

        public override object GetPropertyValue(object instance, string propertyName)
        {
            var type = instance.GetType().FullName;
            Func<object> getProp;
            if (!GetProperties.ContainsKey(type))
                GetProperties[type] = new Dictionary<string, Func<object>>();
            if (GetProperties[type].ContainsKey(propertyName))
            {
                getProp = GetProperties[type][propertyName];
            }
            else
            {
                var propertyInfo = GetPropertyInfo(instance, propertyName);
                getProp = (Func<object>)Delegate.CreateDelegate(instance.GetType(), propertyInfo.GetMethod);
                GetProperties[type][propertyName] = getProp;
            }
            return getProp();
        }
    }
}
