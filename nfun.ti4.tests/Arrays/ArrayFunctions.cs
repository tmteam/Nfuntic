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
        public void Get_Generic()
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

        [Test(Description = "y = [1,2][0]")]
        public void Get_ConstrainsGeneric()
        {
            //     4  2 0,  1  3
            //y = get([ 1, -1],0) 
            var graph = new GraphBuilder();
            graph.SetIntConst(0, PrimitiveType.U8);
            graph.SetIntConst(1, PrimitiveType.I16);
            graph.SetArrayInit(2,0,1);
            graph.SetConst(3, PrimitiveType.I32);
            graph.SetArrGetCall(2, 3, 4);
            graph.SetDef("y", 4);
            var result = graph.Solve();
            var generic = result.AssertAndGetSingleGeneric(PrimitiveType.I16, PrimitiveType.Real);
            result.AssertNamedEqualToArrayOf(generic, "y");

        }

        [Test(Description = "y:char = x[0]")]
        public void Get_ConcreteDef()
        {
            //          2  0,1
            //y:char = get(x,0) 
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetConst(1, PrimitiveType.I32);
            graph.SetArrGetCall(0, 1, 2);
            graph.SetVarType("y", PrimitiveType.Char);
            graph.SetDef("y", 2);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamedEqualToArrayOf(PrimitiveType.Char, "x");
            result.AssertNamed(PrimitiveType.Char, "y");
        }

        [Test(Description = "x:int[]; y = x[0]")]
        public void Get_ConcreteArg()
        {
            //          2  0,1
            //x:int[]; y = get(x,0) 
            var graph = new GraphBuilder();
            graph.SetVarType("x", ArrayOf.Create(PrimitiveType.I32));
            graph.SetVar("x", 0);
            graph.SetConst(1, PrimitiveType.I32);
            graph.SetArrGetCall(0, 1, 2);
            graph.SetDef("y", 2);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamedEqualToArrayOf(PrimitiveType.I32, "x");
            result.AssertNamed(PrimitiveType.I32, "y");
        }

        [Test(Description = "x:int[]; y = x[0]")]
        public void Get_ConcreteArgAndDef_Upcast()
        {
            //          2  0,1
            //x:int[]; y:real = get(x,0) 
            var graph = new GraphBuilder();
            graph.SetVarType("x", ArrayOf.Create(PrimitiveType.I32));
            graph.SetVar("x", 0);
            graph.SetConst(1, PrimitiveType.I32);
            graph.SetArrGetCall(0, 1, 2);
            graph.SetVarType("y", PrimitiveType.Real);
            graph.SetDef("y", 2);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamedEqualToArrayOf(PrimitiveType.I32, "x");
            result.AssertNamed(PrimitiveType.Real, "y");
        }

        [Test(Description = "x:int[]; y = x[0]")]
        public void Get_ConcreteArgAndDef_Impossible()
        {
            try
            {
                //          2  0,1
                //x:real[]; y:int = get(x,0) 
                var graph = new GraphBuilder();
                graph.SetVarType("x", ArrayOf.Create(PrimitiveType.Real));
                graph.SetVar("x", 0);
                graph.SetConst(1, PrimitiveType.I32);
                graph.SetArrGetCall(0, 1, 2);
                graph.SetVarType("y", PrimitiveType.I32);
                graph.SetDef("y", 2);
                var result = graph.Solve();
                Assert.Fail("Impossible equation solved");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                
            }
        }

        [Test(Description = "y = concat(a,b)")]
        public void GenericConcat()
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
        public void ConcreteConcat()
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
