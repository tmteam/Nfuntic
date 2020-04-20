using System;
using System.Collections.Generic;
using System.Text;
using nfun.Ti4;
using NUnit.Framework;

namespace nfun.ti4.tests.UnitTests
{
    class FunTest
    {
        [Test]
        public void ConcreteTypes_SameTypes_EqualsReturnsTrue()
        {
            var funA = Fun.Of(PrimitiveType.Any, PrimitiveType.I32);
            var funB = Fun.Of(PrimitiveType.Any, PrimitiveType.I32);
            Assert.IsTrue(funA.Equals(funB));
        }

        [Test]
        public void ConcreteTypes_DifferentArgs_EqualsReturnsFalse()
        {
            var funA = Fun.Of(PrimitiveType.Any, PrimitiveType.I32);
            var funB = Fun.Of(PrimitiveType.Any, PrimitiveType.Real);
            Assert.IsFalse(funA.Equals(funB));
        }

        [Test]
        public void ConcreteTypes_DifferentReturns_EqualsReturnsFalse()
        {
            var funA = Fun.Of(PrimitiveType.Any, PrimitiveType.I32);
            var funB = Fun.Of(PrimitiveType.Real, PrimitiveType.I32);
            Assert.IsFalse(funA.Equals(funB));
        }
        [Test]
        public void NonConcreteTypes_SameNodes_EqualsReturnsTrue()
        {
            var retNode = CreateConstrainsNode();
            var argNode = CreateConstrainsNode();

            var funA = Fun.Of(retNode, argNode);
            var funB = Fun.Of(retNode, argNode);
            Assert.IsTrue(funA.Equals(funB));
        }
        [Test]
        public void NonConcreteTypes_DifferentNodes_EqualsReturnsTrue()
        {
            var retNodeA = CreateConstrainsNode();
            var retNodeB = CreateConstrainsNode();

            var argNode  = CreateConstrainsNode();

            var funA = Fun.Of(retNodeA, argNode);
            var funB = Fun.Of(retNodeB, argNode);
            Assert.IsFalse(funA.Equals(funB));
        }
        [Test]
        public void ConcreteTypes_IsSolvedReturnsTrue()
        {
            var fun = Fun.Of(PrimitiveType.Any, PrimitiveType.I32);
            Assert.IsTrue(fun.IsSolved);
        }

        [Test]
        public void GenericTypes_IsSolvedReturnsFalse()
        {
            var fun = Fun.Of(CreateConstrainsNode(), CreateConstrainsNode());
            Assert.IsFalse(fun.IsSolved);
        }

        [Test]
        public void GetLastCommonAncestorOrNull_SameConcreteTypes_ReturnsEqualType()
        {
            var funA = Fun.Of(PrimitiveType.Any, PrimitiveType.I32);
            var funB = Fun.Of(PrimitiveType.Any, PrimitiveType.I32);
            var ancestor = funA.GetLastCommonAncestorOrNull(funB);
            Assert.AreEqual(funA, ancestor);
            var ancestor2 = funB.GetLastCommonAncestorOrNull(funA);
            Assert.AreEqual(ancestor2, ancestor);
        }

        [Test]
        public void GetLastCommonAncestorOrNull_ConcreteType_ReturnsAncestor()
        {
            var funA = Fun.Of(PrimitiveType.I32, PrimitiveType.I64);
            var funB = Fun.Of(PrimitiveType.U16, PrimitiveType.U64);
            var expected = Fun.Of(PrimitiveType.I32, PrimitiveType.U48);

            Assert.AreEqual(expected, funA.GetLastCommonAncestorOrNull(funB));
            Assert.AreEqual(expected, funB.GetLastCommonAncestorOrNull(funA));
        }

        [Test]
        public void GetLastCommonAncestorOrNull_NotConcreteTypes_ReturnsNull()
        {
            var funA = Fun.Of(CreateConstrainsNode(), SolvingNode.CreateTypeNode(PrimitiveType.I32));
            var funB = Fun.Of(CreateConstrainsNode(), SolvingNode.CreateTypeNode(PrimitiveType.I32));

            Assert.IsNull(funA.GetLastCommonAncestorOrNull(funB));
            Assert.IsNull(funB.GetLastCommonAncestorOrNull(funA));
        }

        [Test]
        public void GetLastCommonAncestorOrNull_ConcreteAndNotConcreteType_ReturnsNull()
        {
            var funA     = Fun.Of(CreateConstrainsNode(), SolvingNode.CreateTypeNode(PrimitiveType.I32));
            var funB     = Fun.Of(PrimitiveType.U16, PrimitiveType.U64);

            Assert.IsNull(funA.GetLastCommonAncestorOrNull(funB));
            Assert.IsNull(funB.GetLastCommonAncestorOrNull(funA));
        }

        private SolvingNode CreateConstrainsNode()
            => new SolvingNode("", new SolvingConstrains(), SolvingNodeType.TypeVariable);
    }
}
