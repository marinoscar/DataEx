using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilEx
{
    public interface IObjectAccesor : IObjectCreator
    {
        object GetPropertyValue(object target, string propertyName);
        T GetPropertyValue<T>(object target, string propertyName);
        T TryGetPropertyValue<T>(object target, string propertyName);

        void SetPropertyValue(object target, string propertyName, object value);
        void TrySetPropertyValue(object target, string propertyName, object value);
    }
}
