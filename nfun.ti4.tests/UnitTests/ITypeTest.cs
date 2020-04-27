using System;
using System.Collections.Generic;
using System.Text;
using nfun.Ti4;
using nfun.Ti4.SolvingStates;
using NUnit.Framework;
using Array = nfun.Ti4.Array;

namespace nfun.ti4.tests.UnitTests
{
    public class ITypeTest
    {
        [Test]
        public void GetLastCommonAncestorOrNull_ConcreteFunTypesAndPrimitive_ReturnsAny()
        {
            var fun = Fun.Of(Primitive.I32, Primitive.I64);

            Assert.AreEqual(Primitive.Any, fun.GetLastCommonAncestorOrNull(Primitive.I32));
            Assert.AreEqual(Primitive.Any, Primitive.I32.GetLastCommonAncestorOrNull(fun));
        }

        [Test]
        public void GetLastCommonAncestorOrNull_ConcreteFunTypeAndConcreteArray_ReturnsAny()
        {
            var fun = Fun.Of(Primitive.I32, Primitive.I64);
            var array = Array.Of(Primitive.I64);
            Assert.AreEqual(Primitive.Any, fun.GetLastCommonAncestorOrNull(array));
            Assert.AreEqual(Primitive.Any, array.GetLastCommonAncestorOrNull(fun));
        }

        [Test]
        public void GetLastCommonAncestorOrNull_ConcreteFunTypeAndConstrainsArray_ReturnsAny()
        {
            var fun = Fun.Of(Primitive.I32, Primitive.I64);
            var array = Array.Of(CreateConstrainsNode());
            Assert.AreEqual(Primitive.Any, fun.GetLastCommonAncestorOrNull(array));
            Assert.AreEqual(Primitive.Any, array.GetLastCommonAncestorOrNull(fun));
        }


        private SolvingNode CreateConstrainsNode()
            => new SolvingNode("", new Constrains(), SolvingNodeType.TypeVariable);

    }
}
