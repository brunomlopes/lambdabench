using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace LambdaBench
{
    public static class DumpMethod
    {
        public static void Dump(this LambdaExpression lambda)
        {
            new[] {lambda}.Dump();
        }
        public static void Dump(this IEnumerable<LambdaExpression> lambdas)
        {
            var da = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("dyn"), // call it whatever you want
                AssemblyBuilderAccess.Save);
            

            var dm = da.DefineDynamicModule("dyn_mod", "dyn.dll");
            var dt = dm.DefineType("dyn_type");
            foreach ((var lambda, var i) in lambdas.Select((expression, i) => (expression,i)))
            {
                var method = dt.DefineMethod(
                    lambda.Name+$"_{i}",
                    MethodAttributes.Public | MethodAttributes.Static);

                lambda.CompileToMethod(method);

            }
            dt.CreateType();
            
             
            da.Save("dyn.dll");
        }
    }
}