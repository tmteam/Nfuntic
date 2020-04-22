using System.Linq;
using nfun.Ti4;
using NUnit.Framework;

namespace nfun.ti4.tests
{
    public static class TestHelper
    {
        public static void AssertAreGenerics(this FinalizationResults result, SolvingNode targetGenericNode,
            params string[] varNames)
        {
            foreach (var varName in varNames)
            {
                Assert.AreEqual(targetGenericNode, result.GetVariableNode(varName).GetNonReference());
            }
        }

        public static SolvingNode AssertAndGetSingleArithGeneric(this FinalizationResults result)
            => AssertAndGetSingleGeneric(result, PrimitiveType.U24, PrimitiveType.Real, false);

        public static SolvingNode AssertAndGetSingleGeneric(this FinalizationResults result, PrimitiveType desc,
            PrimitiveType anc, bool isComparable = false)
        {
            Assert.AreEqual(1, result.GenericsCount,"Incorrect generics count");
            var genericNode = result.Generics.Single();

            AssertGenericType(genericNode, desc, anc, isComparable);
            return genericNode;
        }

        public static void AssertGenericTypeIsArith(this SolvingNode node)
        {
            AssertGenericType(node, PrimitiveType.U24, PrimitiveType.Real, false);
        }

        public static void AssertGenericType(this SolvingNode node, PrimitiveType desc, PrimitiveType anc,
            bool isComparable = false)
        {
            var generic = node.State as SolvingConstrains;
            Assert.IsNotNull(generic);
            if (desc == null)
                Assert.IsFalse(generic.HasDescendant);
            else
                Assert.AreEqual(desc, generic.Descedant);

            if (anc == null)
                Assert.IsFalse(generic.HasAncestor);
            else
                Assert.AreEqual(anc, generic.Ancestor);

            Assert.AreEqual(isComparable, generic.IsComparable,"IsComparable claim missed");
        }

        public static void AssertNoGenerics(this FinalizationResults results) 
            => Assert.AreEqual(0, results.GenericsCount,"Unexpected generic types");

        public static void AssertNamedEqualToArrayOf(this FinalizationResults results, object typeOrNode, params string[] varNames)
        {
            foreach (var varName in varNames)
            {
                var node = results.GetVariableNode(varName).GetNonReference();
                if (node.State is Array array)
                {
                    var element = array.ElementNode;
                    if (typeOrNode is PrimitiveType concrete)
                        Assert.IsTrue(concrete.Equals(element.State));
                    else
                        Assert.AreEqual(typeOrNode, array.ElementNode);
                }
                else 
                {
                    Assert.Fail();
                }
            }
        }
        public static void AssertNamed(this FinalizationResults results, IType type, params string[] varNames)
        {
            foreach (var varName in varNames)
            {
                Assert.AreEqual(type, results.GetVariableNode(varName).GetNonReference().State);
            }
        }
    }
}