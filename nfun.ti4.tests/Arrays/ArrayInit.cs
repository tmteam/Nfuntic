using nfun.Ti4;
using NUnit.Framework;

namespace nfun.ti4.tests.Arrays
{
    class ArrayInit
    {
        [Test]
        public void ArrayInitWithSpecifiedArrayType()
        {
            //           3 0  1  2 
            // y:int[] = [1i,2i,3i]
            var graph = new GraphBuilder();
            graph.SetVarType("y", Array.Of(PrimitiveType.I32));
            graph.SetConst(0, PrimitiveType.I32);
            graph.SetConst(1, PrimitiveType.I32);
            graph.SetConst(2, PrimitiveType.I32);
            graph.SetArrayInit(3, 0, 1, 2);
            graph.SetDef("y", 3);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamedEqualToArrayOf(PrimitiveType.I32, "y");
        }
        [Test]
        public void ArrayInitWithSpecifiedArrayTypeAndUpcast()
        {
            //            3 0  1  2 
            // y:real[] = [1i,2i,3i]
            var graph = new GraphBuilder();
            graph.SetVarType("y", Array.Of(PrimitiveType.Real));
            graph.SetConst(0, PrimitiveType.I32);
            graph.SetConst(1, PrimitiveType.I32);
            graph.SetConst(2, PrimitiveType.I32);
            graph.SetArrayInit(3, 0, 1, 2);
            graph.SetDef("y", 3);

            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamedEqualToArrayOf(PrimitiveType.Real, "y");
        }

        [Test]
        public void ArrayInitWithSpecifiedArrayTypeAndDowncast_fails()
        {
            try
            {
                //            3 0  1  2 
                // y:byte[] = [1i,2i,3i]
                var graph = new GraphBuilder();
                graph.SetVarType("y", Array.Of(PrimitiveType.U8));
                graph.SetConst(0, PrimitiveType.I32);
                graph.SetConst(1, PrimitiveType.I32);
                graph.SetConst(2, PrimitiveType.I32);
                graph.SetArrayInit(3, 0, 1, 2);
                graph.SetDef("y", 3);
                graph.Solve();
                Assert.Fail("Equation should not be solved");
            }
            catch {}
        }

        [Test]
        public void GenericArrayInitWithSpecifiedArrayType()
        {
            //          3 0 1 2 
            // y:int[] = [1,2,3]
            var graph = new GraphBuilder();
            graph.SetVarType("y", Array.Of(PrimitiveType.I32));
            graph.SetIntConst(0, PrimitiveType.U8);
            graph.SetIntConst(1, PrimitiveType.U8);
            graph.SetIntConst(2, PrimitiveType.U8);
            graph.SetArrayInit(3, 0, 1, 2);
            graph.SetDef("y", 3);

            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamedEqualToArrayOf(PrimitiveType.I32, "y");
        }
        [Test]
        public void GenericArrayInit()
        {
            //    3 0 1 2 
            // y = [1,2,3]
            var graph = new GraphBuilder();
            graph.SetIntConst(0, PrimitiveType.U8);
            graph.SetIntConst(1, PrimitiveType.U8);
            graph.SetIntConst(2, PrimitiveType.U8);
            graph.SetArrayInit(3, 0,1,2);
            graph.SetDef("y", 3);

            var result = graph.Solve();
            var generic = result.AssertAndGetSingleGeneric(PrimitiveType.U8, PrimitiveType.Real);
            result.AssertNamedEqualToArrayOf(generic, "y");
        }

        [Test]
        public void GenericArrayInitWithVariable()
        {
            //    3 0 1 2 
            // y = [1,2,x]
            var graph = new GraphBuilder();
            graph.SetIntConst(0, PrimitiveType.U8);
            graph.SetIntConst(1, PrimitiveType.U8);
            graph.SetVar("x",2);
            graph.SetArrayInit(3, 0, 1, 2);
            graph.SetDef("y",3);

            var result = graph.Solve();
            var generic = result.AssertAndGetSingleGeneric(PrimitiveType.U8, PrimitiveType.Real);
            result.AssertNamedEqualToArrayOf(generic, "y");
            result.AssertAreGenerics(generic, "x");
        }
        [Test]
        public void GenericArrayInitWithTwoVariables()
        {
            //    2 0 1  
            // y = [a,b]
            var graph = new GraphBuilder();
            graph.SetVar("a", 0);
            graph.SetVar("b", 1);
            graph.SetArrayInit(2, 0, 1);
            graph.SetDef("y", 2);

            var result = graph.Solve();
            var generic = result.AssertAndGetSingleGeneric(null, null);
            result.AssertNamedEqualToArrayOf(generic, "y");
            result.AssertAreGenerics(generic, "a","b");
        }
        [Test]
        public void GenericArrayInitWithTwoVariablesOneOfThemHasConcreteType()
        {
            //       2 0 1  
            //a:int; y = [a,b]
            var graph = new GraphBuilder();
            graph.SetVarType("a", PrimitiveType.I32);
            graph.SetVar("a", 0);
            graph.SetVar("b", 1);
            graph.SetArrayInit(2, 0, 1);
            graph.SetDef("y", 2);

            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamedEqualToArrayOf(PrimitiveType.I32, "y");
            result.AssertNamed(PrimitiveType.I32, "a", "b");
        }
        [Test]
        public void GenericArrayInitWithComplexVariables()
        {
            //    3 0  21  
            // y = [x,-x]
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetVar("x", 1);
            graph.SetNegateCall(1, 2);
            graph.SetArrayInit(3, 0, 2);
            graph.SetDef("y", 3);

            var result = graph.Solve();
            var generic = result.AssertAndGetSingleGeneric(PrimitiveType.I16, PrimitiveType.Real);
            result.AssertNamedEqualToArrayOf(generic, "y");
            result.AssertAreGenerics(generic, "x");
        }
        [Test]
        public void GenericArrayInitWithTwoSameVariables()
        {
            //    2 0 1  
            // y = [x,x]
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetVar("x", 1);
            graph.SetArrayInit(2, 0, 1);
            graph.SetDef("y", 2);

            var result = graph.Solve();
            var generic = result.AssertAndGetSingleGeneric(null, null);
            result.AssertNamedEqualToArrayOf(generic, "y");
            result.AssertAreGenerics(generic, "x");
        }


        [Test]
        public void ArrayInitWithConcreteConstant()
        {
            //    3 0 1 2 
            // y = [1.0,2,3]
            var graph = new GraphBuilder();
            graph.SetConst(0, PrimitiveType.Real);
            graph.SetIntConst(1, PrimitiveType.U8);
            graph.SetIntConst(2, PrimitiveType.U8);
            graph.SetArrayInit(3, 0, 1, 2);
            graph.SetDef("y", 3);
        
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamedEqualToArrayOf(PrimitiveType.Real, "y");
        }

        [Test]
        public void TwoDimention_InitConcrete()
        {
            //     4 3 0 1 2 
            // y = [[1i,2i,3i]]
            var graph = new GraphBuilder();
            graph.SetConst(0, PrimitiveType.I32);
            graph.SetConst(1, PrimitiveType.I32);
            graph.SetConst(2, PrimitiveType.I32);
            graph.SetArrayInit(3, 0, 1, 2);
            graph.SetArrayInit(4,3);
            graph.SetDef("y", 4);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(Array.Of(Array.Of(PrimitiveType.I32)), "y");
        }

        [Test]
        public void TwoDimention_InitConcrete_ConcreteDef()
        {
            //             4 3 0 1 2 
            // y:int[][] = [[1i,2i,3i]]
            var graph = new GraphBuilder();
            graph.SetConst(0, PrimitiveType.I32);
            graph.SetConst(1, PrimitiveType.I32);
            graph.SetConst(2, PrimitiveType.I32);
            graph.SetArrayInit(3, 0, 1, 2);
            graph.SetArrayInit(4, 3);
            graph.SetVarType("y", Array.Of(Array.Of(PrimitiveType.I32)));
            graph.SetDef("y", 4);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(Array.Of(Array.Of(PrimitiveType.I32)), "y");
        }
    }
}
