using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilEx
{
    public class ClassHelper<T> where T : class
    {
        #region Variable Declaration

        private readonly Type _targetType;
        private readonly Func<T> _instanceCreator;

        #endregion

        #region Constructors

        public ClassHelper()
        {
            _targetType = typeof(T);
            _instanceCreator = GetConstructor();
            PropertyInfos = new Dictionary<string, PropertyInfo>();
            SetPropertyDic = new Dictionary<string, Action<object, object>>();
            GetPropertyDic = new Dictionary<string, Func<object>>();
            var properties = _targetType.GetProperties();
            foreach (var propertyInfo in properties)
            {
                PropertyInfos[propertyInfo.Name] = propertyInfo;
            }
        } 

        #endregion

        #region Property Implementation

        private Dictionary<string, Action<object, object>> SetPropertyDic { get; set; }
        private Dictionary<string, Func<object>> GetPropertyDic { get; set; }
        public Dictionary<string, PropertyInfo> PropertyInfos { get; private set; }

        #endregion

        #region Method Implementation

        private Func<T> GetConstructor()
        {
            var constructorInfo = _targetType.GetConstructor(new Type[] { });
            var constructor = Expression.New(constructorInfo, null);
            var newItem = Expression.Lambda(typeof(Func<T>), constructor, null);
            return (Func<T>)newItem.Compile();
        }


        public T FromDictionary(Dictionary<string, object> values)
        {
            var item = CreateInstance();
            foreach (var key in values.Keys)
            {
                SetPropertyValue(item, key, values[key]);
            }
            return item;
        }

        public T FromDataRecord(IDataRecord dataRecord)
        {
            var item = CreateInstance();
            for (var i = 0; i < dataRecord.FieldCount; i++)
            {
                var columnName = dataRecord.GetName(i);
                SetPropertyValue(item, columnName, dataRecord.GetValue(i));
            }
            return item;
        }

        public Dictionary<string, object> ToDictionary()
        {
            var dic = new Dictionary<string, object>();
            if (GetPropertyDic != null)
            {
                foreach (var key in GetPropertyDic.Keys)
                {
                    dic[key] = GetPropertyDic[key]();
                }
            }
            return dic;
        }

        public T CreateInstance()
        {
            return _instanceCreator();
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

        public void SetPropertyValue<TValue>(T target, string propertyName, TValue value)
        {
            SetPropertyValue<TValue>(target, PropertyInfos[propertyName], value);
        }

        public void SetPropertyValue<TValue>(T target, PropertyInfo propertyInfo, TValue value)
        {
            Action<object, object> setProp;
            if (SetPropertyDic.ContainsKey(propertyInfo.Name))
                setProp = SetPropertyDic[propertyInfo.Name];
            else
            {
                setProp = CreateSetDelegate(propertyInfo);
                SetPropertyDic[propertyInfo.Name] = setProp;
            }
            setProp(target, value);
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

        public TValue GetPropertyValue<TValue>(T target, PropertyInfo propertyInfo)
        {
            var getProp = GetPropertyDic[propertyInfo.Name];
            if (getProp == null)
            {
                getProp = (Func<object>)Delegate.CreateDelegate(_targetType, propertyInfo.GetMethod);
                GetPropertyDic[propertyInfo.Name] = getProp;
            }
            return (TValue)getProp();
        }

        public TValue GetPropertyValue<TValue>(T target, string propertyName)
        {
            return GetPropertyValue<TValue>(target, PropertyInfos[propertyName]);
        }

        #endregion
    }
}
