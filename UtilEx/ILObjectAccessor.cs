using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UtilEx
{
    public class IlObjectAccessor : PropertyInfoBasedAccessor
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
            throw new NotImplementedException();
        }

        public override object GetPropertyValue(object instance, string propertyName)
        {
            throw new NotImplementedException();
        }
    }
}
