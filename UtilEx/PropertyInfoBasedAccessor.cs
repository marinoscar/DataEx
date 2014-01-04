using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilEx
{
    public abstract class PropertyInfoBasedAccessor : ObjectAccessor
    {
        private static Dictionary<string, Dictionary<string, PropertyInfo>> _propertyInfos;

        protected static Dictionary<string, Dictionary<string, PropertyInfo>> PropertyInfos
        {
            get { return _propertyInfos ?? (_propertyInfos = new Dictionary<string, Dictionary<string, PropertyInfo>>()); }
        }

        protected static PropertyInfo GetPropertyInfo(object instance, string propertyName)
        {
            var type = instance.GetType();
            if (!PropertyInfos.ContainsKey(type.FullName))
                PropertyInfos[type.FullName] = new Dictionary<string, PropertyInfo>();
            if (PropertyInfos.ContainsKey(type.FullName) && PropertyInfos[type.FullName].ContainsKey(propertyName))
            {
                return PropertyInfos[type.FullName][propertyName];
            }
            var property = type.GetProperty(propertyName);
            if (property == null)
                throw new ArgumentException("Object of type {0} does not contained property {1}".Fi(type, propertyName));
            PropertyInfos[type.FullName][propertyName] = property;
            return property;
        }
    }
}
