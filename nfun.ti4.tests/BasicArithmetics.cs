using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using nfun.Ti4;
using NUnit.Framework;

namespace nfun.ti4.tests
{
    class BasicArithmetics
    {
        [TestCase]
        public void IncrementI64()
        {
            Console.WriteLine("y = x + 1i");

            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetConst(1, ConcreteType.I64);
            graph.SetArith2(0, 1, 2);
            graph.SetDef("y", 2);

            var result = graph.SolveTypes();

            Assert.AreEqual(0, result.GenericsCount);

            Assert.AreEqual(ConcreteType.I64, result.GetVariable("x"));
            Assert.AreEqual(ConcreteType.I64, result.GetVariable("y"));
        }

        
        [TestCase]
        public void GenericIncrement()
        {
            Console.WriteLine("y = x + 1");

            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetIntConst(1, ConcreteType.U8);
            graph.SetArith2(0, 1, 2);
            graph.SetDef("y", 2);

            var result = graph.SolveTypes();

            Assert.AreEqual(1, result.GenericsCount);
            var genericNode = result.Generics.Single();

            var generic = genericNode.NodeState as SolvingConstrains;
            Assert.AreEqual(ConcreteType.Real, generic.CommonAncestor);
            Assert.AreEqual(ConcreteType.U24, generic.CommonDescedant);
            Assert.IsFalse(generic.IsComparable);

            Assert.AreEqual(genericNode, result.GetVariableNode("x").GetNonReference());
            Assert.AreEqual(genericNode, result.GetVariableNode("y").GetNonReference());
        }

        [Test]
        public void StrictOnEquationArithmetics()
        {
            Console.WriteLine("x= 10i;   a = x*y + 10-x");

            var graph = new GraphBuilder();
            graph.SetConst(0, ConcreteType.I32);
            graph.SetDef("x", 0);

            graph.SetVar("x", 1);
            graph.SetVar("y", 2);
            graph.SetArith2(1, 2, 3);
            graph.SetIntConst(4, ConcreteType.U8);
            graph.SetArith2(3, 4, 5);
            graph.SetVar("x", 6);
            graph.SetArith2(5, 6, 7);
            graph.SetDef("a", 7);


            var result = graph.SolveTypes();
            Assert.AreEqual(0, result.GenericsCount);

            Assert.AreEqual(ConcreteType.I32, result.GetVariable("x"));
            Assert.AreEqual(ConcreteType.I32, result.GetVariable("y"));
            Assert.AreEqual(ConcreteType.I32, result.GetVariable("a"));
        }

        [Test]
        public void GenericOneEquatopmArithmetics()
        {
            Console.WriteLine("x= 10;   a = x*y + 10-x");

            var graph = new GraphBuilder();
            graph.SetIntConst(0, ConcreteType.U8);
            graph.SetDef("x", 0);

            graph.SetVar("x", 1);
            graph.SetVar("y", 2);
            graph.SetArith2(1, 2, 3);
            graph.SetIntConst(4, ConcreteType.U8);
            graph.SetArith2(3, 4, 5);
            graph.SetVar("x", 6);
            graph.SetArith2(5, 6, 7);
            graph.SetDef("a", 7);

            var result = graph.SolveTypes();

            Assert.AreEqual(1, result.GenericsCount);
            var genericNode = result.Generics.Single();

            var generic = genericNode.NodeState as SolvingConstrains;
            Assert.AreEqual(ConcreteType.Real, generic.CommonAncestor);
            Assert.AreEqual(ConcreteType.U24, generic.CommonDescedant);
            Assert.IsFalse(generic.IsComparable);

            Assert.AreEqual(genericNode, result.GetVariableNode("x").GetNonReference());
            Assert.AreEqual(genericNode, result.GetVariableNode("y").GetNonReference());
            Assert.AreEqual(genericNode, result.GetVariableNode("a").GetNonReference());

        }
        [Test]
        public void GenericTwoEquationsArithmetic()
        {
            Console.WriteLine("a = x*y + 10-x; b = r*x + 10-r");

            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetVar("y", 1);
            graph.SetArith2(0, 1, 2);
            graph.SetIntConst(3, ConcreteType.U8);
            graph.SetArith2(2, 3, 4);
            graph.SetVar("x", 5);
            graph.SetArith2(4, 5, 6);
            graph.SetDef("a", 6);

            graph.SetVar("r", 7);
            graph.SetVar("x", 8);
            graph.SetArith2(7, 8, 9);
            graph.SetIntConst(10, ConcreteType.U8);
            graph.SetArith2(9, 10, 11);
            graph.SetVar("r", 12);
            graph.SetArith2(11, 12, 13);
            graph.SetDef("b", 13);

            var result = graph.SolveTypes();

            Assert.AreEqual(1, result.GenericsCount);
            var genericNode = result.Generics.Single();

            var generic = genericNode.NodeState as SolvingConstrains;
            Assert.AreEqual(ConcreteType.Real, generic.CommonAncestor);
            Assert.AreEqual(ConcreteType.U24, generic.CommonDescedant);
            Assert.IsFalse(generic.IsComparable);

            Assert.AreEqual(genericNode, result.GetVariableNode("x").GetNonReference());
            Assert.AreEqual(genericNode, result.GetVariableNode("y").GetNonReference());
            Assert.AreEqual(genericNode, result.GetVariableNode("a").GetNonReference());
            Assert.AreEqual(genericNode, result.GetVariableNode("b").GetNonReference());
        }

    }
}
