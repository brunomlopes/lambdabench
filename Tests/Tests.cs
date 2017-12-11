using LambdaBench;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class Tests
    {
        [Fact]
        public void MethodCallBenchsTest()
        {
            var x = new MethodCallBenchs();
            x.GlobalSetup();
            x.WithActionFromExpression();
        }

        [Fact]
        public void Something()
        {
            var l = new LambdaVersusInvoke();

            foreach (var i in Enumerable.Range(0, 10000))
                l.CallViaCachedExpressionWithTwoArgumentsWithoutInvoke();

            foreach (var i in Enumerable.Range(0, 10000))
                l.CallViaAction();

            foreach (var i in Enumerable.Range(0, 10000))
                l.Regularcall();
        }
    }
}
