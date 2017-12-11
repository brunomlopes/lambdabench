using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace LambdaBench
{
    //[SimpleJob(launchCount: 1, warmupCount: 3, targetCount: 5, invocationCount: 1_000)]
    public class LambdaVersusInvoke
    {
        private static MethodInfo AddWhereAllowedIdsContainsMethodInfo =
            typeof(LambdaVersusInvoke).GetMethod(nameof(AddWhereAllowedIdsContains), BindingFlags.NonPublic | BindingFlags.Static);

        private static IDictionary<Type, MethodInfo> WhereMethodInfoCache = new Dictionary<Type, MethodInfo>();
        private Mock<LambdaVersusInvoke> instance;


        public LambdaVersusInvoke()
        {
            instance = new Mock<LambdaVersusInvoke>();
            ActionToCall = ((obj, str) => AddWhereAllowedIdsContains(instance, str));
        }

        [Benchmark]
        public void Regularcall()
        {
            AddWhereAllowedIdsContains(instance, "users/42");
        }


        [Benchmark]
        public void CallViaExpression()
        {

            var qType = instance.GetType().GenericTypeArguments[0];
            Expression.Lambda<Action>(Expression.Call(
                    AddWhereAllowedIdsContainsMethodInfo.MakeGenericMethod(qType),
                    Expression.Constant(instance),
                    Expression.Constant("users/42")))
                .Compile()
                .Invoke();
        }


        ParameterExpression userIdArg = Expression.Parameter(typeof(string));

        [Benchmark]
        public void CallViaExpressionWithArgument()
        {
            var userId = "users/42";
            var qType = instance.GetType().GenericTypeArguments[0];
            Expression.Lambda<Action<string>>(Expression.Call(
                        AddWhereAllowedIdsContainsMethodInfo.MakeGenericMethod(qType),
                        Expression.Constant(instance), userIdArg),
                    userIdArg)
                .Compile()
                .Invoke(userId);
        }

        [Benchmark]
        public void CallViaExpressionWithTwoArguments()
        {
            var userId = "users/42";
            var instanceType = instance.GetType();
            var qType = instanceType.GenericTypeArguments[0];
            var typeArg = Expression.Parameter(typeof(object));
            Expression.Lambda<Action<object, string>>(Expression.Call(
                        AddWhereAllowedIdsContainsMethodInfo.MakeGenericMethod(qType),
                        Expression.Convert(typeArg, instanceType), userIdArg),
                    typeArg, userIdArg)
                .Compile()
                .Invoke(instance, userId);
        }

        [Benchmark]
        public void CallViaCachedExpressionnWithTwoArguments()
        {
            var userId = "users/42";
            var instanceType = instance.GetType();
            var action = GetActionForInstance(instanceType);
            action.Invoke(instance, userId);
        }
        [Benchmark]
        public void CallViaCachedExpressionWithTwoArgumentsWithoutInvoke()
        {
            var userId = "users/42";
            var instanceType = instance.GetType();
            var action = GetActionForInstance(instanceType);
            action(instance, userId);
        }

        private Action<object, string> ActionToCall ;


        [Benchmark]
        public Type CallViaAction()
        {
            var userId = "users/42";
            var instanceType = instance.GetType();
            ActionToCall(instance, userId);
            return instanceType;
        }

        private static IDictionary<Type, Action<object, string>> CachedExpressions 
            = new Dictionary<Type, Action<object, string>>();

        private Action<object, string> GetActionForInstance(Type instanceType)
        {
            var qType = instanceType.GenericTypeArguments[0];
            if (!CachedExpressions.ContainsKey(qType))
            {
                var typeArgForCache = Expression.Parameter(typeof(object));

                var action = Expression.Lambda<Action<object, string>>(Expression.Call(
                            AddWhereAllowedIdsContainsMethodInfo.MakeGenericMethod(qType),
                            Expression.Convert(typeArgForCache, instanceType), userIdArg),
                        typeArgForCache, userIdArg)
                    .Compile();
                CachedExpressions[qType] = action;
            }
            return CachedExpressions[qType];
        }



        //[Benchmark]
        //public void CallViaCachedExpressionToMethodBuilderWithTwoArgumentsWithoutInvoke()
        //{
        //    var userId = "users/42";
        //    var instanceType = instance.GetType();
        //    var i = GetActionForInstanceViaMethodBuilder(instanceType);
        //    i.AddWhere(instance, userId);
        //}

        //private static IDictionary<Type, ICallerInterface> CachedExpressionsMethodBuilder
        //    = new Dictionary<Type, ICallerInterface>();

        //private ICallerInterface  GetActionForInstanceViaMethodBuilder(Type instanceType)
        //{
        //    var qType = instanceType.GenericTypeArguments[0];
        //    if (!CachedExpressionsMethodBuilder.ContainsKey(qType))
        //    {
                
        //        var typeArgForCache = Expression.Parameter(typeof(object));
                
        //        var expression = Expression.Lambda<Action<object, string>>(Expression.Call(
        //                    AddWhereAllowedIdsContainsMethodInfo.MakeGenericMethod(qType),
        //                    Expression.Convert(typeArgForCache, instanceType), userIdArg),
        //                typeArgForCache, userIdArg);

        //        var type = LinqRuntimeTypeBuilder.GetDynamicType(qType.Name, expression);
        //        var x = (ICallerInterface)Activator.CreateInstance(type);

                    
        //        CachedExpressionsMethodBuilder[qType] = x;
        //    }
        //    return CachedExpressionsMethodBuilder[qType];
        //}

        [Benchmark]
        public void CallViaInvoke()
        {
            var qType = instance.GetType().GenericTypeArguments[0];
            AddWhereAllowedIdsContainsMethodInfo
                .MakeGenericMethod(qType)
                .Invoke(null, new object[]{ instance , "users/42"});
        }

        [Benchmark]
        public void CallViaCachedInvoke()
        {
            var qType = instance.GetType().GenericTypeArguments[0];

            if (!WhereMethodInfoCache.ContainsKey(qType))
            {
                WhereMethodInfoCache[qType] = AddWhereAllowedIdsContainsMethodInfo.MakeGenericMethod(qType);
            }

            WhereMethodInfoCache[qType]
                .Invoke(null, new object[]{ instance , "users/42"});
        }



        // TODO: I think we might be able to just toss a function serverside to load the roles into the query
        // Yeah, i know, doing a query for each query is really suboptimal. 
        private static void AddWhereAllowedIdsContains<T>(Mock<T> query, string userId)
        {
            var x = userId.Length;
            var y = x + 2;
        }

        class Mock<T>
        {
            
        }
    }
}