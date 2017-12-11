using System;
using System.Linq.Expressions;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;

namespace LambdaBench
{
    [SimpleJob(launchCount: 1, warmupCount: 3, targetCount: 5, invocationCount: 1_000_000)]
    public class MethodCallBenchs
    {
        Mock _instance;
        public class Mock
        {
            public string Name { get; set; }
        }

        [GlobalSetup]
        public void GlobalSetup()
        {

            action = mock => DoThingsWithMock(mock);

            Expression<Action<Mock>> expressionForActionFromDirectExpression = mock => DoThingsWithMock(mock);
            actionFromDirectExpression = expressionForActionFromDirectExpression.Compile();

            var parameter = Expression.Parameter(typeof(Mock));
            doThingsWithMockMethodInfo = typeof(MethodCallBenchs).GetMethod("DoThingsWithMock");

            var expression = Expression.Lambda<Action<Mock>>(
                Expression.Call(doThingsWithMockMethodInfo, parameter),
                "expressionFromLambda",
                new[] {parameter});

            //new[] {expressionForActionFromDirectExpression, expression}.Dump();
            actionFromExpression = expression
                .Compile();
            
            _instance = new Mock() { Name = "A Random Name" };
        }

        [Benchmark(Baseline = true)]
        public void DirectCall()
        {
            DoThingsWithMock(_instance);
        }


        [Benchmark]
        public void CallThroughInstanceMethod()
        {
            CallDoThingsWithMock(_instance);
        }

        [Benchmark]
        public void CallThroughVirtualInstanceMethod()
        {
            VirtualCallDoThingsWithMock(_instance);
        }

        static Action<Mock> action;
        [Benchmark]
        public void WithAction()
        {
            action(_instance);
        }

        static Action<Mock> actionFromDirectExpression;
        [Benchmark]
        public void WithActionFromDirectExpression()
        {
            actionFromDirectExpression(_instance);
        }

        static Action<Mock> actionFromExpression;
        [Benchmark]
        public void WithActionFromExpression()
        {
            actionFromExpression(_instance);
        }

        static MethodInfo doThingsWithMockMethodInfo;
        [Benchmark]
        public void Invoke()
        {
            doThingsWithMockMethodInfo.Invoke(null, new[] { _instance });
        }

        public virtual int VirtualCallDoThingsWithMock(Mock mock)
        {
            return DoThingsWithMock(mock);
        }


        public int CallDoThingsWithMock(Mock mock)
        {
            return DoThingsWithMock(mock);
        }

        public static int DoThingsWithMock(Mock mock)
        {
            return mock.Name.Length;
        }
    }
}