using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilEx
{
    public class ObjectHelper
    {
        private static Dictionary<Type, Dictionary<string, PropertyInfo>> _propertyCache;

        public static T CreateInstance<T>()
        {
            return Activator.CreateInstance<T>();
        }

        public static void SetPropertyValue(object item, string propertyName, object value)
        {
            var property = GetProperty(item, propertyName);
            if (property == null) return;
            var valueType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            var safeValue = (value == DBNull.Value || value == null) ? null : Convert.ChangeType(value, valueType);
            property.SetValue(item, safeValue);
        }

        public static object GetPropertyValue(object item, string propertyName)
        {
            var property = GetProperty(item, propertyName);
            return property == null ? null : property.GetValue(item);
        }

        private static PropertyInfo GetProperty(object item, string propertyName)
        {
            var type = item.GetType();
            if (_propertyCache == null)
                _propertyCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
            if (!_propertyCache.ContainsKey(type))
                _propertyCache[type] = new Dictionary<string, PropertyInfo>();
            if (!_propertyCache[type].ContainsKey(propertyName))
            {
                var propertyInfo = type.GetProperty(propertyName);
                _propertyCache[type][propertyName] = propertyInfo;
                return propertyInfo;
            }
            return _propertyCache[type][propertyName];
        }
    }
}
