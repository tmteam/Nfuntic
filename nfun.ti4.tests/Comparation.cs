using System;
using System.Collections.Generic;
using System.Text;
using nfun.Ti4;
using NUnit.Framework;

namespace nfun.ti4.tests
{
    class Comparation
    {
        [Test]
        public void CompareTwoVariables()
        {
            //      0 2 1
            // y =  a > b

            var graph = new GraphBuilder();
            graph.SetVar("a",0);
            graph.SetVar("b", 1);
            graph.SetComparable(0,1,2);
            graph.SetDef("y",2);

            var result = graph.Solve();
            
            result.AssertNamed(ConcreteType.Bool, "y");
            var generic = result.AssertAndGetSingleGeneric(null, null, true);
            result.AssertAreGenerics(generic, "a", "b");
        }

        [Test]
        public void CompareVariableAndConstant()
        {
            //      0 2 1
            // y =  a > 1i

            var graph = new GraphBuilder();
            graph.SetVar("a", 0);
            graph.SetConst(1, ConcreteType.I32);
            graph.SetComparable(0, 1, 2);
            graph.SetDef("y", 2);

            var result = graph.Solve();

            result.AssertNamed(ConcreteType.Bool, "y");
            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.I32, "a");
        }

        [Test]
        public void CompareConstants()
        {
            //      0  2 1
            // y =  2i > 1i

            var graph = new GraphBuilder();
            graph.SetConst(0, ConcreteType.I32);
            graph.SetConst(1, ConcreteType.I32);
            graph.SetComparable(0, 1, 2);
            graph.SetDef("y", 2);

            var result = graph.Solve();

            result.AssertNamed(ConcreteType.Bool, "y");
            result.AssertNoGenerics();
        }

        [Test]
        public void CompareTwoDifferentConstants()
        {
            //      0   2 1
            // y =  2.0 > 1i

            var graph = new GraphBuilder();
            graph.SetConst(0, ConcreteType.Real);
            graph.SetConst(1, ConcreteType.I32);
            graph.SetComparable(0, 1, 2);
            graph.SetDef("y", 2);

            var result = graph.Solve();

            result.AssertNamed(ConcreteType.Bool, "y");
            result.AssertNoGenerics();
        }

        [Test]
        [Ignore("Generic comparation not implemented yet")]
        public void CompareTwoDifferentUncomparableConstants()
        {
            //      0   2  1
            // y =  2.0 > 'v'

            var graph = new GraphBuilder();
            graph.SetConst(0, ConcreteType.Real);
            graph.SetConst(1, ConcreteType.Char);

            Assert.Throws<InvalidOperationException>(() =>
            {
                graph.SetComparable(0, 1, 2);
                graph.SetDef("y", 2);

                graph.Solve();
            });
        }
    }
}
