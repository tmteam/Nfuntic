using System;
using System.Collections.Generic;
using System.Text;
using nfun.Ti4;
using NUnit.Framework;

namespace nfun.ti4.tests
{
    class BitwiseOperations
    {
        [Test]
        public void BitwiseConstants()
        {
            //    0  2 1
            //y = 1u & 2u
            var graph = new GraphBuilder();
            graph.SetConst(0, ConcreteType.U32);
            graph.SetConst(1, ConcreteType.U32);
            graph.SetBitwise(0,1,2);
            graph.SetDef("y",2);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.U32, "y");
        }
       
        [Test]
        [Ignore("Todo - сделать оптимизацию для поиска подходящего типа")]
        public void BitwiseDifferentConstants()
        {
            //    0  2 1
            //y = 1u & 2i
            var graph = new GraphBuilder();
            graph.SetConst(0, ConcreteType.U32);
            graph.SetConst(1, ConcreteType.I32);
            graph.SetBitwise(0, 1, 2);
            graph.SetDef("y", 2);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.I64, "y");
        }


        [Test]
        public void BitwiseGenericConstants()
        {
            //    0 2 1
            //y = 1 & 2
            var graph = new GraphBuilder();
            graph.SetIntConst(0, ConcreteType.U8);
            graph.SetIntConst(1, ConcreteType.U8);
            graph.SetBitwise(0, 1, 2);
            graph.SetDef("y", 2);
            var result = graph.Solve();
            var generic = result.AssertAndGetSingleGeneric(ConcreteType.U8, ConcreteType.I96);
            result.AssertAreGenerics(generic, "y");
        }
        [Test]
        public void BitwiseGenericAndConstant()
        {
            //    0 2 1
            //y = 1 & x
            var graph = new GraphBuilder();
            graph.SetIntConst(0, ConcreteType.U8);
            graph.SetVar("x",1);
            graph.SetBitwise(0, 1, 2);
            graph.SetDef("y", 2);
            var result = graph.Solve();
            var generic = result.AssertAndGetSingleGeneric(ConcreteType.U8, ConcreteType.I96);
            result.AssertAreGenerics(generic, "x","y");
        }

        [Test]
        public void BitwiseNamedAndConstant()
        {
            //    0 2 1
            //y = 1i & x
            var graph = new GraphBuilder();
            graph.SetConst(0, ConcreteType.I32);
            graph.SetVar("x", 1);
            graph.SetBitwise(0, 1, 2);
            graph.SetDef("y", 2);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.I32, "x","y");
        }

        [Test]
        public void BitwiseComplexGenericEquation()
        {
            //    0 2 1 4   3 6 5
            //y = 1 & x | 256 | a
            var graph = new GraphBuilder();
            graph.SetIntConst(0, ConcreteType.U8);
            graph.SetVar("x", 1);
            graph.SetBitwise(0, 1, 2);
            graph.SetIntConst(3, ConcreteType.U12);
            graph.SetBitwise(2, 3, 4);
            graph.SetVar("a", 5);
            graph.SetBitwise(4, 5, 6);
            graph.SetDef("y", 6);
            var result = graph.Solve();
            var generic = result.AssertAndGetSingleGeneric(ConcreteType.U12, ConcreteType.I96);
            result.AssertAreGenerics(generic, "a", "x", "y");
        }

        [Test]
        public void BitwiseComplexGenericEquation2()
        {
            //    0 2 1 4   3 6 5
            //y = 1 & x | -1 | a
            var graph = new GraphBuilder();
            graph.SetIntConst(0, ConcreteType.U8);
            graph.SetVar("x", 1);
            graph.SetBitwise(0, 1, 2);
            graph.SetIntConst(3, ConcreteType.I16);
            graph.SetBitwise(2, 3, 4);
            graph.SetVar("a", 5);
            graph.SetBitwise(4, 5, 6);
            graph.SetDef("y", 6);
            var result = graph.Solve();
            var generic = result.AssertAndGetSingleGeneric(ConcreteType.I16, ConcreteType.I96);
            result.AssertAreGenerics(generic, "a", "x", "y");
        }
    }
}
