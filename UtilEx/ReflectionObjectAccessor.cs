using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilEx
{
    public class ReflectionObjectAccessor : PropertyInfoBasedAccessor
    {
        public override object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }

        public override T CreateInstance<T>()
        {
            return Activator.CreateInstance<T>();
        }

        public override void SetPropertyValue(object instance, string propertyName, object value)
        {
            var property = GetPropertyInfo(instance, propertyName);
            property.SetValue(instance, value);
        }

        public override object GetPropertyValue(object instance, string propertyName)
        {
            var property = GetPropertyInfo(instance, propertyName);
            return property.GetValue(instance);
        }
    }
}
