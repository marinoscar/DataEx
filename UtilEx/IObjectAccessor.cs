using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilEx
{
    public interface IObjectAccessor
    {
        object CreateInstance(Type type);
        T CreateInstance<T>();
        void SetPropertyValue(object instance, string propertyName, object value);
        object GetPropertyValue(object instance, string propertyName);
        T GetPropertyValue<T>(object instance, string propertyName);
    }
}
