using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nfun.Ti4.Toposort
{
    public static class NodeToposortFunctions
    {
        public static (SolvingNode[], TopologySortResults) Toposort(SolvingNode[] nodes)
        {
            var graph = ConvertToArrayGraph(nodes);

            var sorted = GraphTools.SortTopology(graph);
            var order = sorted.NodeNames.Select(n => nodes[n.To]).Reverse().ToArray();
            return (order, sorted);
        }
        public static Edge[][] ConvertToArrayGraph(SolvingNode[] allNodes)
        {
            var graph = new LinkedList<Edge>[allNodes.Length];
            for (int i = 0; i < allNodes.Length; i++)
                allNodes[i].GraphId = i;

            for (int i = 0; i < allNodes.Length; i++)
            {
                var node = allNodes[i];
                if (node.MemberOf.Any())
                {
                    foreach (var arrayNode in node.MemberOf)
                    {
                        PutEdges(arrayNode.GraphId, node);
                    }
                }
                else
                {
                    PutEdges(i, node);
                }
            }

            return graph.Select(g => g?.ToArray()).ToArray();

            void PutEdges(int @from, SolvingNode source)
            {
                if (graph[@from] == null)
                    graph[@from] = new LinkedList<Edge>();

                foreach (var anc in source.Ancestors)
                {
                    graph[from].AddLast(Edge.AncestorTo(anc.GraphId));
                }
                if (source.State is RefTo reference)
                {
                    var to = reference.Node.GraphId;
                    if (graph[to] == null)
                        graph[to] = new LinkedList<Edge>();

                    //Two nodes references each other
                    graph[from].AddLast(Edge.ReferenceTo(to));
                    graph[to].AddLast(Edge.ReferenceTo(from));
                }
            }
        }
    }
}
