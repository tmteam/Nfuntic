using System;
using nfun.Ti4;
using NUnit.Framework;

namespace nfun.ti4.tests.Arrays
{
    public class ArrayGetCallTest
    {
        [Test(Description = "y = x[0]")]
        public void Generic()
        {
            //     2  0,1
            //y = get(x,0) 
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetConst(1, PrimitiveType.I32);
            graph.SetArrGetCall(0, 1, 2);
            graph.SetDef("y", 2);
            var result = graph.Solve();
            var generic = result.AssertAndGetSingleGeneric(null, null);
            result.AssertNamedEqualToArrayOf(generic, "x");
            result.AssertAreGenerics(generic, "y");
        }

        [Test(Description = "y = [1,2][0]")]
        public void ConstrainsGeneric()
        {
            //     4  2 0,  1  3
            //y = get([ 1, -1],0) 
            var graph = new GraphBuilder();
            graph.SetIntConst(0, PrimitiveType.U8);
            graph.SetIntConst(1, PrimitiveType.I16);
            graph.SetArrayInit(2, 0, 1);
            graph.SetConst(3, PrimitiveType.I32);
            graph.SetArrGetCall(2, 3, 4);
            graph.SetDef("y", 4);
            var result = graph.Solve();
            var generic = result.AssertAndGetSingleGeneric(PrimitiveType.I16, PrimitiveType.Real);
            result.AssertAreGenerics(generic, "y");

        }

        [Test(Description = "y:char = x[0]")]
        public void ConcreteDef()
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
        public void ConcreteArg()
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
        public void ConcreteArgAndDef_Upcast()
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
        public void ConcreteArgAndDef_Impossible()
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

        [Test(Description = "y = x[0][0]")]
        public void TwoDimentions_Generic()
        {
            //    4    2  0,1  3
            //y = get(get(x,0),0) 
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetConst(1, PrimitiveType.I32);
            graph.SetArrGetCall(0, 1, 2);
            graph.SetConst(3, PrimitiveType.I32);
            graph.SetArrGetCall(2, 3, 4);
            graph.SetDef("y", 4);
            var result = graph.Solve();
            var generic = result.AssertAndGetSingleGeneric(null, null);
            result.AssertNamed(ArrayOf.Create(new ArrayOf(generic)), "x");
            result.AssertAreGenerics(generic, "y");
        }


        [Test(Description = "y:int = x[0][0]")]
        public void TwoDimentions_ConcreteDef()
        {
            //    4    2  0,1  3
            //y:int = get(get(x,0),0) 
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetConst(1, PrimitiveType.I32);
            graph.SetArrGetCall(0, 1, 2);
            graph.SetConst(3, PrimitiveType.I32);
            graph.SetArrGetCall(2, 3, 4);
            graph.SetVarType("y", PrimitiveType.I32);
            graph.SetDef("y", 4);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(ArrayOf.Create(ArrayOf.Create(PrimitiveType.I32)), "x");
            result.AssertNamed(PrimitiveType.I32, "y");
        }

        [Test(Description = "x:int[][]; y = x[0][0]")]
        public void TwoDimentions_ConcreteArg()
        {
            //    4    2  0,1  3
            //x:int[][]; y = get(get(x,0),0) 
            var graph = new GraphBuilder();
            graph.SetVarType("x", ArrayOf.Create(ArrayOf.Create(PrimitiveType.I32)));
            graph.SetVar("x", 0);
            graph.SetConst(1, PrimitiveType.I32);
            graph.SetArrGetCall(0, 1, 2);
            graph.SetConst(3, PrimitiveType.I32);
            graph.SetArrGetCall(2, 3, 4);
            graph.SetDef("y", 4);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(ArrayOf.Create(ArrayOf.Create(PrimitiveType.I32)), "x");
            result.AssertNamed(PrimitiveType.I32, "y");
        }

        [Test(Description = "x:int[][]; y:int = x[0][0]")]
        public void TwoDimentions_ConcreteArgAndDef()
        {
            //                   4    2  0,1  3
            //x:int[][]; y:int = get(get(x,0),0) 
            var graph = new GraphBuilder();
            graph.SetVarType("x", ArrayOf.Create(ArrayOf.Create(PrimitiveType.I32)));
            graph.SetVar("x", 0);
            graph.SetConst(1, PrimitiveType.I32);
            graph.SetArrGetCall(0, 1, 2);
            graph.SetConst(3, PrimitiveType.I32);
            graph.SetArrGetCall(2, 3, 4);
            graph.SetVarType("y", PrimitiveType.I32);
            graph.SetDef("y", 4);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(ArrayOf.Create(ArrayOf.Create(PrimitiveType.I32)), "x");
            result.AssertNamed(PrimitiveType.I32, "y");
        }

        [Test(Description = "x:int[][]; y:real = x[0][0]")]
        public void TwoDimentions_ConcreteArgAndDefWithUpcast()
        {
            //                    4    2  0,1  3
            //x:int[][]; y:real = get(get(x,0),0) 
            var graph = new GraphBuilder();
            graph.SetVarType("x", ArrayOf.Create(ArrayOf.Create(PrimitiveType.I32)));
            graph.SetVar("x", 0);
            graph.SetConst(1, PrimitiveType.I32);
            graph.SetArrGetCall(0, 1, 2);
            graph.SetConst(3, PrimitiveType.I32);
            graph.SetArrGetCall(2, 3, 4);
            graph.SetVarType("y", PrimitiveType.Real);
            graph.SetDef("y", 4);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(ArrayOf.Create(ArrayOf.Create(PrimitiveType.I32)), "x");
            result.AssertNamed(PrimitiveType.Real, "y");
        }

        [Test(Description = "x:int[][]; y:i16 = x[0][0]")]
        public void TwoDimentions_ImpossibleConcreteArgAndDef()
        {
            //                   4    2  0,1  3
            //x:int[][]; y:i16 = get(get(x,0),0) 
            var graph = new GraphBuilder();
            graph.SetVarType("x", ArrayOf.Create(ArrayOf.Create(PrimitiveType.I32)));
            graph.SetVar("x", 0);
            graph.SetConst(1, PrimitiveType.I32);
            graph.SetArrGetCall(0, 1, 2);
            graph.SetConst(3, PrimitiveType.I32);
            graph.SetArrGetCall(2, 3, 4);
            try
            {
                graph.SetVarType("y", PrimitiveType.I16);
                graph.SetDef("y", 4);
                graph.Solve();
                Assert.Fail();
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        [Test(Description = "x:int[][]; y:i16 = x[0][0]")]
        public void ThreeDimentions_ConcreteDefArrayOf()
        {
            //           4    2  0,1  3
            //y:real[] = get(get(x,0),0) 
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetConst(1, PrimitiveType.I32);
            graph.SetArrGetCall(0, 1, 2);
            graph.SetConst(3, PrimitiveType.I32);
            graph.SetArrGetCall(2, 3, 4);
            graph.SetVarType("y", ArrayOf.Create(PrimitiveType.Real));
            graph.SetDef("y", 4);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(ArrayOf.Create(ArrayOf.Create(ArrayOf.Create(PrimitiveType.Real))), "x");
            result.AssertNamed(ArrayOf.Create(PrimitiveType.Real), "y");
        }

    }
}