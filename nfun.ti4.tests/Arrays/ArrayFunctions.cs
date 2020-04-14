using System;
using System.Collections.Generic;
using System.Text;
using nfun.Ti4;
using NUnit.Framework;

namespace nfun.ti4.tests.Arrays
{
    public class ArrayFunctions
    {
        [Test(Description = "y = x[0]")]
        public void Get()
        {
            //     2  0,1
            //y = get(x,0) 
            var graph = new GraphBuilder();
            graph.SetVar("x",0);
            graph.SetConst(1, PrimitiveType.I32);
            graph.SetArrGetCall(0, 1, 2);
            graph.SetDef("y",2);
            var result = graph.Solve();
            var generic = result.AssertAndGetSingleGeneric(null, null);
            result.AssertNamedEqualToArrayOf(generic, "x");
            result.AssertAreGenerics(generic, "y");

        }

        [Test(Description = "y = concat(a,b)")]
        public void Concat()
        {
            //     2     0 1
            //y = concat(a,b) 
            var graph = new GraphBuilder();
            graph.SetVar("a", 0);
            graph.SetVar("b", 1);
            graph.SetConcatCall(0, 1, 2);
            graph.SetDef("y", 2);
            var result = graph.Solve();
            var generic = result.AssertAndGetSingleGeneric(null, null);
            result.AssertNamedEqualToArrayOf(generic, "a","b","y");
        }


        [Test(Description = "y = concat(a,b)")]
        public void ConcatConcreteType()
        {
            //              2     0 1
            //a:int[]; y = concat(a,b) 
            var graph = new GraphBuilder();
            graph.SetVarType("a", ArrayOf.Create(PrimitiveType.I32));
            graph.SetVar("a", 0);
            graph.SetVar("b", 1);
            graph.SetConcatCall(0, 1, 2);
            graph.SetDef("y", 2);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamedEqualToArrayOf(ArrayOf.Create(PrimitiveType.I32),"y","a","b");
        }
    }
}
