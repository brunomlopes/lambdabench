using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes.Jobs;

namespace LambdaBench
{
    class Program
    {
        static void Main(string[] args)
        {
            
            //BenchmarkRunner.Run<LambdaVersusInvoke>();
            BenchmarkRunner.Run<MethodCallBenchs>();

            //new LambdaVersusInvoke().CallViaExpressionWithTwoArguments();
        }
    }
}
