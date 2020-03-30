using nfun.Ti4;
using NUnit.Framework;

namespace nfun.ti4.tests
{
    public class Tests
    {

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var graph = new GraphBuilder();

            //y = x + 1

            graph.SetVar("x",0);
            graph.SetIntConst(1, SolvingNode.U8);
            graph.SetArith(0,1,2);
            graph.SetDef("y",2);
        }
    }
}