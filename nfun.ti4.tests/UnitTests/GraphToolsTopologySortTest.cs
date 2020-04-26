using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using nfun.Ti4;
using nfun.Ti4.Toposort;
using NUnit.Framework;

namespace nfun.ti4.tests.UnitTests
{
    [TestFixture]
    public class GraphToolsTopologySortTest
    {
        #region ancestorsOnly
        [Test]
        public void OneNodeCycle()
        {
            var graph = new[]
            {
                From(0),
            };
            var res = GraphTools.SortTopology(graph);
            AssertHasCycle(new[] { 0 }, res);
        }
        [Test]
        public void TwoNodesCycle()
        {
            var graph = new[]
            {
                From(1),
                From(0),
            };
            var res = GraphTools.SortTopology(graph);
            AssertHasCycle(new[] { 0, 1 }, res);
        }
        [Test]
        public void ThreeNodesCycle()
        {
            var graph = new[]
            {
                From(3),
                From(0),
                From(1),
                From(2),
            };
            var res = GraphTools.SortTopology(graph);
            AssertHasCycle(new[] { 0, 1, 2, 3 }, res);
        }
        [Test]
        public void ComplexNodesCycle()
        {
            //         |<------|
            //0->1->2->|3->4->5|->6
            var graph = new[]
            {
                NoParents,
                From(0),
                From(1),
                From(2,5), //cycle here
                From(3),
                From(4),
                From(5),
                From(6),
            };
            var res = GraphTools.SortTopology(graph);
            AssertHasCycle(new[] { 3, 4, 5 }, res);
        }
        [Test]
        public void TwoNodesGraphSorting()
        {
            //1->0
            var graph = new[]
            {
                From(1),
                NoParents
            };
            var res = GraphTools.SortTopology(graph);
            AssertHasRoute(new[] { 1, 0 }, res);
        }
        [Test]
        public void ThreeNodesInLineSorting()
        {
            //2->1->0
            var graph = new[]
            {
                From(1),
                From(2),
                NoParents
            };
            var res = GraphTools.SortTopology(graph);
            AssertHasRoute(new[] { 2, 1, 0 }, res);
        }
        [Test]
        public void ThreeNodesInLineRevertSorting()
        {
            //0->1->2
            var graph = new[]
            {
                NoParents,
                From(0),
                From(1)
            };
            var res = GraphTools.SortTopology(graph);
            AssertHasRoute(new[] { 0, 1, 2 }, res);
        }
        [Test]
        public void ComplexGraphSorting()
        {
            //{5,3}->|6->|
            //   {1,4} ->|0->2
            var graph = new[]
            {
                From(1,4,6),
                NoParents,
                From(0),
                NoParents,
                NoParents,
                NoParents,
                From(5,3),
            };
            var res = GraphTools.SortTopology(graph);
            AssertHasRoute(new[] { 1, 4, 5, 3, 6, 0, 2 }, res);
        }
        [Test]
        public void SeveralComplexGraphSorting()
        {
            var graph = new Edge[17][];

            //{5,3}->|6->|
            //   {1,4} ->|0->2
            graph[0] = From(1, 4, 6);
            graph[1] = NoParents;
            graph[2] = From(0);
            graph[3] = NoParents;
            graph[4] = NoParents;
            graph[5] = NoParents;
            graph[6] = From(5, 3);
            //{12,8}->|10->|
            //    {9,11} ->|13->7
            graph[7] = From(13);
            graph[8] = NoParents;
            graph[9] = NoParents;
            graph[10] = From(12, 8);
            graph[11] = NoParents;
            graph[12] = NoParents;
            graph[13] = From(9, 11, 10);
            //14
            graph[14] = NoParents;
            //15->16
            graph[15] = NoParents;
            graph[16] = From(15);

            var res = GraphTools.SortTopology(graph);
            AssertHasRoute(new[]
            {
                1,4,5,3,6,0,2,
                9,11,12,8,10,13,7,
                14,
                15,16
            }, res);
        }
        [Test]
        public void ThreeSeparatedNodesSorting()
        {
            //2,1,0
            var graph = new[]
            {
                NoParents,
                NoParents,
                NoParents
            };
            var res = GraphTools.SortTopology(graph);
            AssertHasRoute(new[] { 0, 1, 2 }, res);
        }
        #endregion

        [Test]
        public void ReferenceCycle_cycleFound()
        {
            //2=1=0
            //|===|  

            var graph = new[]
            {
                EqualsTo(1,2),
                EqualsTo(2,0),
                EqualsTo(1,0),
            };
            var res = GraphTools.SortTopology(graph);
            AssertHasCycle(new[] { 0, 2, 1 }, res);
        }
        [Test]
        public void Reference_NoCycles()
        {
            //0==1

            var graph = new[]
            {
                EqualsTo(1),
                EqualsTo(0),
            };
            var res = GraphTools.SortTopology(graph);
            AssertHasRoute(new []{1,0}, res);
        }
        [Test]
        public void ThreeReferencesInARow_NoCycles()
        {
            //0==1==2

            var graph = new[]
            {
                EqualsTo(1),
                EqualsTo(0,2),
                EqualsTo(1)
            };
            var res = GraphTools.SortTopology(graph);
            AssertHasRoute(new[] { 2,1,0}, res);
        }

        private Edge[] NoParents => new Edge[0];
        private Edge[] From(params int[] routes) 
            => routes
                .Select(r=>new Edge(r, EdgeType.Ancestor))
                .ToArray();
        private Edge[] EqualsTo(params int[] routes)
            => routes
                .Select(r => new Edge(r, EdgeType.Equal))
                .ToArray();

        private string ArrayToString(IEnumerable<int> arr) => $"[{string.Join(",", arr)}]";
        private string ArrayToString(IEnumerable<Edge> arr) => $"[{string.Join(" ", arr)}]";
        private void AssertHasCycle(int[] cycle, TopologySortResults actual)
        {
            Assert.IsTrue(actual.HasCycle, "Cycle not found");
            CollectionAssert.AreEqual(cycle, actual.NodeNames.Select(n=>n.To).ToArray(),
                $"expected: {ArrayToString(cycle)} but was: {ArrayToString(actual.NodeNames)}");

        }
        private void AssertHasRoute(int[] expected, TopologySortResults actual)
        {
            Assert.IsFalse(actual.HasCycle, "Order not found");
            CollectionAssert.AreEqual(expected, actual.NodeNames.Select(n=>n.To).ToArray(),
                $"expected: {ArrayToString(expected)} but was: {ArrayToString(actual.NodeNames)}");

        }
    }
}
