using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilEx
{
    public abstract class ObjectAccessor : IObjectAccessor
    {
        public abstract object CreateInstance(Type type);
        public abstract T CreateInstance<T>();
        public abstract void SetPropertyValue(object instance, string propertyName, object value);
        public abstract object GetPropertyValue(object instance, string propertyName);

        
        public T GetPropertyValue<T>(object instance, string propertyName)
        {
            var val = GetPropertyValue(instance, propertyName);
            return (T)Convert.ChangeType(val, typeof (T));
        }
    }
}
