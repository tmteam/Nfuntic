using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using nfun.Ti4;
using NUnit.Framework;

namespace nfun.ti4.tests
{
    class Primitives
    {
        [Test(Description = "y = x ")]
        public void OutputEqualsInput_simpleGeneric()
        {
            //node |1   0
            //expr |y = x 
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetDef("y", 0);
            var result = graph.Solve();

            var generic = result.AssertAndGetSingleGeneric(null, null, false);
            result.AssertAreGenerics(generic, "x", "y");
        }

        [Test(Description = "y = x; | y2 = x2")]
        public void TwoSimpleGenerics()
        {
            //node |     0  |       1
            //expr s|y = x; | y2 = x2
            
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetDef("y", 0);

            graph.SetVar("x2", 1);
            graph.SetDef("y2", 1);

            var result = graph.Solve();

            Assert.AreEqual(2, result.GenericsCount);

            var generics = result.Generics.ToArray();

            generics[0].AssertGenericType(null, null, false);
            generics[1].AssertGenericType(null, null, false);

            var yRes = result.GetVariableNode("y").GetNonReference();
            var y2Res = result.GetVariableNode("y2").GetNonReference();
            CollectionAssert.AreEquivalent(generics, new[]{y2Res, yRes});

            var xRes = result.GetVariableNode("x").GetNonReference();
            var x2Res = result.GetVariableNode("x2").GetNonReference();
            CollectionAssert.AreEquivalent(generics, new[] { x2Res, xRes });

        }

    }
}
