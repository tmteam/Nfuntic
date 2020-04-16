using System;
using nfun.Ti4;
using NUnit.Framework;

namespace nfun.ti4.tests.Arrays
{
    public class ArraySumCallTest
    {
        [Test(Description = "y = x.sum()")]
        public void Generic()
        {
            //     1  0
            //y = sum(x) 
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetSumCall(0, 1);
            graph.SetDef("y", 1);
            var result = graph.Solve();
            var generic = result.AssertAndGetSingleArithGeneric();
            result.AssertNamedEqualToArrayOf(generic, "x");
            result.AssertAreGenerics(generic, "y");
        }

        [Test(Description = "y = [1,-1].sum()")]
        public void ConstrainsGeneric()
        {
            //     3  2 0,  1  
            //y = sum([ 1, -1]) 
            var graph = new GraphBuilder();
            graph.SetIntConst(0, PrimitiveType.U8);
            graph.SetIntConst(1, PrimitiveType.I16);
            graph.SetArrayInit(2, 0, 1);
            graph.SetSumCall(2,3);
            graph.SetDef("y", 3);
            var result = graph.Solve();
            var generic = result.AssertAndGetSingleGeneric(PrimitiveType.I32, PrimitiveType.Real);
            result.AssertAreGenerics(generic, "y");
        }

        [Test(Description = "y:u32 = x.sum()")]
        public void ConcreteDefType()
        {
            //         1  0
            //y:u32 = sum(x) 
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetSumCall(0, 1);
            graph.SetVarType("y", PrimitiveType.U32);
            graph.SetDef("y", 1);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamedEqualToArrayOf(PrimitiveType.U32, "x");
            result.AssertNamed(PrimitiveType.U32, "y");
        }

        [Test(Description = "y:char = x.sum()")]
        public void ImpossibleDefType_Throws()
        {
            //          1  0
            //y:char = sum(x) 

            var graph = new GraphBuilder();
            try
            {
                graph.SetVar("x", 0);
                graph.SetSumCall(0, 1);
                graph.SetVarType("y", PrimitiveType.Char);
                graph.SetDef("y", 1);
                graph.Solve();
                Assert.Fail();
            }
            catch (Exception e) 
            {
                Console.WriteLine(e);
            }
            
        }

        [Test(Description = "x:int[]; y = x.sum()")]
        public void ConcreteArg()
        {
            //               2 0
            //x:int[]; y = sum(x) 
            var graph = new GraphBuilder();
            graph.SetVarType("x", ArrayOf.Create(PrimitiveType.I32));
            graph.SetVar("x", 0);
            graph.SetSumCall(0, 1);
            graph.SetDef("y", 1);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamedEqualToArrayOf(PrimitiveType.I32, "x");
            result.AssertNamed(PrimitiveType.I32, "y");
        }

        [Test(Description = "x:int[]; y:real = x.sum()")]
        public void ConcreteArgAndDef_Upcast()
        {
            //                   2  0
            //x:int[]; y:real = sum(x) 
            var graph = new GraphBuilder();
            graph.SetVarType("x", ArrayOf.Create(PrimitiveType.I32));
            graph.SetVar("x", 0);
            graph.SetSumCall(0, 1);
            graph.SetVarType("y", PrimitiveType.Real);
            graph.SetDef("y", 1);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamedEqualToArrayOf(PrimitiveType.I32, "x");
            result.AssertNamed(PrimitiveType.Real, "y");
        }

        [Test(Description = "x:real[]; y:int = x[0]")]
        public void Impossible_ConcreteArgAndDef_throws()
        {
            try
            {
                //                   1  0
                //x:real[]; y:int = sum(x) 
                var graph = new GraphBuilder();
                graph.SetVarType("x", ArrayOf.Create(PrimitiveType.Real));
                graph.SetVar("x", 0);
                graph.SetSumCall(0, 1);
                graph.SetVarType("y", PrimitiveType.I32);
                graph.SetDef("y", 1);
                var result = graph.Solve();
                Assert.Fail("Impossible equation solved");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }
        }
    }
}