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

            for (int from = 0; from < allNodes.Length; from++)
            {
                var node = allNodes[@from];

                if (graph[@from] == null)
                    graph[@from] = new LinkedList<Edge>();

                foreach (var anc in node.Ancestors)
                {
                    graph[from].AddLast(Edge.AncestorTo(anc.GraphId));
                }
                if (node.State is RefTo reference)
                {
                    var to = reference.Node.GraphId;
                    if (graph[to] == null)
                        graph[to] = new LinkedList<Edge>();

                    //Two nodes references each other
                    graph[from].AddLast(Edge.ReferenceTo(to));
                    graph[to].AddLast(Edge.ReferenceTo(from));
                }
                else if (node.State is ICompositeType composite)
                {
                    foreach (var member in composite.Members)
                    {
                        if(member.GraphId<0)
                            continue;
                        var mfrom = member.GraphId;

                        if (graph[mfrom] == null)
                            graph[mfrom] = new LinkedList<Edge>();
                        graph[mfrom].AddLast(Edge.MemberOf(from));
                    }
                }
            }

            return graph.Select(g => g?.ToArray()).ToArray();
        }
    }
}
