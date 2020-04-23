using System;
using System.Collections.Generic;
using System.Text;
using nfun.Ti4;
using NUnit.Framework;
using Array = nfun.Ti4.Array;

namespace nfun.ti4.tests.UnitTests
{
    public class ITypeTest
    {
        [Test]
        public void GetLastCommonAncestorOrNull_ConcreteFunTypesAndPrimitive_ReturnsAny()
        {
            var fun = Fun.Of(PrimitiveType.I32, PrimitiveType.I64);

            Assert.AreEqual(PrimitiveType.Any, fun.GetLastCommonAncestorOrNull(PrimitiveType.I32));
            Assert.AreEqual(PrimitiveType.Any, PrimitiveType.I32.GetLastCommonAncestorOrNull(fun));
        }

        [Test]
        public void GetLastCommonAncestorOrNull_ConcreteFunTypeAndConcreteArray_ReturnsAny()
        {
            var fun = Fun.Of(PrimitiveType.I32, PrimitiveType.I64);
            var array = Array.Of(PrimitiveType.I64);
            Assert.AreEqual(PrimitiveType.Any, fun.GetLastCommonAncestorOrNull(array));
            Assert.AreEqual(PrimitiveType.Any, array.GetLastCommonAncestorOrNull(fun));
        }

        [Test]
        public void GetLastCommonAncestorOrNull_ConcreteFunTypeAndConstrainsArray_ReturnsAny()
        {
            var fun = Fun.Of(PrimitiveType.I32, PrimitiveType.I64);
            var array = Array.Of(CreateConstrainsNode());
            Assert.AreEqual(PrimitiveType.Any, fun.GetLastCommonAncestorOrNull(array));
            Assert.AreEqual(PrimitiveType.Any, array.GetLastCommonAncestorOrNull(fun));
        }


        private SolvingNode CreateConstrainsNode()
            => new SolvingNode("", new Constrains(), SolvingNodeType.TypeVariable);

    }
}
