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
            => AssertAndGetSingleGeneric(result, ConcreteType.U24, ConcreteType.Real, false);

        public static SolvingNode AssertAndGetSingleGeneric(this FinalizationResults result, ConcreteType desc,
            ConcreteType anc, bool isComparable = false)
        {
            Assert.AreEqual(1, result.GenericsCount);
            var genericNode = result.Generics.Single();

            AssertGenericType(genericNode, desc, anc, isComparable);
            return genericNode;
        }

        public static void AssertGenericTypeIsArith(this SolvingNode node)
        {
            AssertGenericType(node, ConcreteType.U24, ConcreteType.Real, false);
        }

        public static void AssertGenericType(this SolvingNode node, ConcreteType desc, ConcreteType anc,
            bool isComparable = false)
        {
            var generic = node.NodeState as SolvingConstrains;
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

        public static void AssertNamed(this FinalizationResults results, ConcreteType type, params string[] varNames)
        {
            foreach (var varName in varNames)
            {
                Assert.AreEqual(type, results.GetVariableNode(varName).GetNonReference().NodeState);
            }
        }
    }
}