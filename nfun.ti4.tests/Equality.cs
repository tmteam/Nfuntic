using System;
using System.Collections.Generic;
using System.Text;
using nfun.Ti4;
using NUnit.Framework;

namespace nfun.ti4.tests
{
    class Equality
    {
        [Test]
        public void TwoVariableEquality()
        {
            //     0  2 1
            // y = a == b
            var graph = new GraphBuilder();
            graph.SetVar("a",0);
            graph.SetVar("b", 1);
            graph.SetEquality(0,1,2);
            graph.SetDef("y",2);

            var result = graph.Solve();
            
            result.AssertNamed(ConcreteType.Bool,"y");
            var generic = result.AssertAndGetSingleGeneric(null, null);
            result.AssertAreGenerics(generic, "a", "b");
        }

        [Test]
        public void VariableAndConstEquality()
        {
            //     0  2 1
            // y = a == 1i
            var graph = new GraphBuilder();
            graph.SetVar("a", 0);
            graph.SetConst(1, ConcreteType.I32);
            graph.SetEquality(0, 1, 2);
            graph.SetDef("y", 2);

            var result = graph.Solve();

            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.Bool, "y");
            result.AssertNamed(ConcreteType.I32, "a");
        }
        [Test]
        public void ConstEquality()
        {
            //     0  2 1
            // y = 1i == 1i
            var graph = new GraphBuilder();
            graph.SetConst(0, ConcreteType.I32);
            graph.SetConst(1, ConcreteType.I32);
            graph.SetEquality(0, 1, 2);
            graph.SetDef("y", 2);

            var result = graph.Solve();

            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.Bool, "y");
        }

        [Test]
        public void DifferentTypesEquality()
        {
            //     0   2 1
            // y = 1i == 1.0
            var graph = new GraphBuilder();
            graph.SetConst(0, ConcreteType.I32);
            graph.SetConst(1, ConcreteType.Real);
            graph.SetEquality(0, 1, 2);
            graph.SetDef("y", 2);

            var result = graph.Solve();

            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.Bool, "y");
        }
    }
}
