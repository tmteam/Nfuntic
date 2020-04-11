using System;
using System.Collections.Generic;
using System.Text;
using nfun.Ti4;
using NUnit.Framework;

namespace nfun.ti4.tests
{
    class ArrayInit
    {
        [Test]
        public void ArrayInitWithSpecifiedArrayType()
        {
            //           3 0  1  2 
            // y:int[] = [1i,2i,3i]
            var graph = new GraphBuilder();
            graph.SetVarType("y", ConcreteType.ArrayOf(ConcreteType.I32));
            graph.SetConst(0, ConcreteType.I32);
            graph.SetConst(1, ConcreteType.I32);
            graph.SetConst(2, ConcreteType.I32);
            graph.SetArrayInit(3, 0, 1, 2);
            graph.SetDef("y", 3);
            graph.Solve();
        }
        [Test]
        public void ArrayInitWithSpecifiedArrayTypeAndUpcast()
        {
            //            3 0  1  2 
            // y:real[] = [1i,2i,3i]
            var graph = new GraphBuilder();
            graph.SetVarType("y", ConcreteType.ArrayOf(ConcreteType.Real));
            graph.SetConst(0, ConcreteType.I32);
            graph.SetConst(1, ConcreteType.I32);
            graph.SetConst(2, ConcreteType.I32);
            graph.SetArrayInit(3, 0, 1, 2);
            graph.SetDef("y", 3);
            graph.Solve();
        }

        [Test]
        public void ArrayInitWithSpecifiedArrayTypeAndDowncast_fails()
        {
            try
            {
                //            3 0  1  2 
                // y:byte[] = [1i,2i,3i]
                var graph = new GraphBuilder();
                graph.SetVarType("y", ConcreteType.ArrayOf(ConcreteType.U8));
                graph.SetConst(0, ConcreteType.I32);
                graph.SetConst(1, ConcreteType.I32);
                graph.SetConst(2, ConcreteType.I32);
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
            graph.SetVarType("y", ConcreteType.ArrayOf(ConcreteType.I32));
            graph.SetIntConst(0, ConcreteType.U8);
            graph.SetIntConst(1, ConcreteType.U8);
            graph.SetIntConst(2, ConcreteType.U8);
            graph.SetArrayInit(3, 0, 1, 2);
            graph.SetDef("y", 3);
            graph.Solve();
        }
        [Test]
        public void GenericArrayInit()
        {
            //    3 0 1 2 
            // y = [1,2,3]
            var graph = new GraphBuilder();
            graph.SetIntConst(0, ConcreteType.U8);
            graph.SetIntConst(1, ConcreteType.U8);
            graph.SetIntConst(2, ConcreteType.U8);
            graph.SetArrayInit(3, 0,1,2);
            graph.SetDef("y", 3);
            graph.Solve();
        }

        [Test]
        public void GenericArrayInitWithVariable()
        {
            //    3 0 1 2 
            // y = [1,2,x]
            var graph = new GraphBuilder();
            graph.SetIntConst(0, ConcreteType.U8);
            graph.SetIntConst(1, ConcreteType.U8);
            graph.SetVar("x",2);
            graph.SetArrayInit(3, 0, 1, 2);
            graph.SetDef("y",3);
            graph.Solve();
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
            graph.Solve();
        }
        [Test]
        public void GenericArrayInitWithTwoVariablesOneOfThemHasConcreteType()
        {
            //       2 0 1  
            //a:int; y = [a,b]
            var graph = new GraphBuilder();
            graph.SetVarType("a", ConcreteType.I32);
            graph.SetVar("a", 0);
            graph.SetVar("b", 1);
            graph.SetArrayInit(2, 0, 1);
            graph.SetDef("y", 2);
            graph.Solve();
        }
        [Test]
        public void GenericArrayInitWithComplexVariables()
        {
            //    3 0  21  
            // y = [x,-x]
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetVar("x", 1);
            graph.SetNegate(1, 2);
            graph.SetArrayInit(3, 0, 2);
            graph.SetDef("y", 3);
            graph.Solve();
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
            graph.Solve();
        }


        [Test]
        public void ArrayInitWithConcreteConstant()
        {
            //    3 0 1 2 
            // y = [1.0,2,3]
            var graph = new GraphBuilder();
            graph.SetConst(0, ConcreteType.Real);
            graph.SetIntConst(1, ConcreteType.U8);
            graph.SetIntConst(2, ConcreteType.U8);
            graph.SetArrayInit(3, 0, 1, 2);
            graph.SetDef("y", 3);
            graph.Solve();
        }
    }
}
