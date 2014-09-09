using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace UtilEx
{
    public class EntityCreator : IObjectCreator
    {
        delegate object MethodInvoker();

        private static Dictionary<string, MethodInvoker> _constructorCache;

        private static MethodInvoker GetConstructor(Type type)
        {
            if (_constructorCache == null) _constructorCache = new Dictionary<string, MethodInvoker>();
            var key = type.FullName;
            if (!_constructorCache.ContainsKey(key))
            {
                var constructor = GetMethod(type.GetConstructor(Type.EmptyTypes));
                _constructorCache.Add(key, constructor);
                return constructor;
            }
            return _constructorCache[key];
        }

        public static object Create(Type type)
        {
            var method = GetConstructor(type);
            return method == null ? null : method();
        }

        T IObjectCreator.Create<T>()
        {
            return EntityCreator.Create<T>();
        }

        object IObjectCreator.Create(Type type)
        {
            return EntityCreator.Create(type);
        }

        public static T Create<T>()
        {
            return (T)Create(typeof (T));
        }




        private static MethodInvoker GetMethod(ConstructorInfo target)
        {
            var dynamic = new DynamicMethod(string.Empty,
                    typeof(object),
                    new Type[0],
                    target.DeclaringType);
            var il = dynamic.GetILGenerator();
            il.DeclareLocal(target.DeclaringType);
            il.Emit(OpCodes.Newobj, target);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            return (MethodInvoker)dynamic.CreateDelegate(typeof(MethodInvoker));
        }
    }
}
