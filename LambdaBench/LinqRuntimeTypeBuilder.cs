using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace LambdaBench
{
    interface ICallerInterface
    {
        void AddWhere(object query, string userId);
    }

    public static class LinqRuntimeTypeBuilder
    {
        private static readonly AssemblyName AssemblyName = new AssemblyName() { Name = "DynamicLinqTypes" };
        private static readonly ModuleBuilder ModuleBuilder = null;
        private static readonly Dictionary<string, Type> BuiltTypes = new Dictionary<string, Type>();
        private static int _typeCounter = 0;

        static LinqRuntimeTypeBuilder()
        {
            ModuleBuilder = Thread.GetDomain().DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run).DefineDynamicModule(AssemblyName.Name);
        }

        //private static string GetTypeKey(Dictionary<string, Type> fields)
        //{
        //    string key = string.Empty;
        //    foreach (var field in fields)
        //        key += field.Key + ";" + field.Value.Name + ";";

        //    return key;
        //}

        public static Type GetDynamicType(string name, Expression<Action<object,string>> method)
        {
            if (null == method)
                throw new ArgumentNullException(nameof(method));

            Type iface = typeof(ICallerInterface);

            try
            {
                Monitor.Enter(BuiltTypes);
                var className = GetClassName();
                var typeKey = name;

                if (BuiltTypes.ContainsKey(typeKey))
                    return BuiltTypes[typeKey];

                var typeBuilder = ModuleBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Serializable, null);
                typeBuilder.AddInterfaceImplementation(iface);


                var field = method;

                {
                    var methodBuilder = typeBuilder.DefineMethod(nameof(ICallerInterface.AddWhere),
                         MethodAttributes.Public, 
                        
                        typeof(void),
                        field.Parameters.Select(p => p.Type).ToArray());
                    
                    field.CompileToMethod(methodBuilder);
                    typeBuilder.DefineMethodOverride(methodBuilder, 
                        typeof(ICallerInterface).GetMethod(nameof(ICallerInterface.AddWhere)));
                }


                return BuiltTypes[typeKey] = typeBuilder.CreateType();
            }
            finally
            {
                Monitor.Exit(BuiltTypes);
            }
        }

        private static string GetClassName()
        {
            _typeCounter += 1;
            return "<>f__AnAnonymousType" + _typeCounter + "`1";
        }


        //public static Type GetDynamicType(IEnumerable<PropertyInfo> fields)
        //{
        //    return GetDynamicType(fields.ToDictionary(f => f.Name, f => f.PropertyType));
        //}
    }
}