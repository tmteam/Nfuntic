using System;
using System.Collections.Generic;

namespace nfun.Ti4
{
    enum NodeState
    {
        NotVisited,
        Checked,
        Checking,
    }
    public struct Edge
    {
        public readonly int To;
        public readonly EdgeType Type;

        public Edge(int to, EdgeType type)
        {
            To = to;
            Type = type;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case EdgeType.Root:     return "!!!" + To;
                case EdgeType.Ancestor: return "::>"+To;
                case EdgeType.Equal:    return "<=>"+To;
                case EdgeType.MemberOf: return "-->" + To;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Edge e))
                return false;
            return Equals(e);
        }

        public bool Equals(Edge other) 
            => To == other.To && Type == other.Type;

        public override int GetHashCode()
        {
            unchecked
            {
                return (To * 397) ^ (int) Type;
            }
        }
    }

    public enum EdgeType
    {
        Root,
        Ancestor,
        Equal,
        MemberOf
    }
    public static class GraphTools
    {

        /// <summary>
        /// Gets topology sorted in form of indexes [ParentNodeName ->  ChildrenNames[] ]
        /// O(N)
        /// If Circular dependencies found -  returns circle route insead of sorted order
        /// </summary>
        /// <returns>topology sorted node indexes from source to drain or first cycle route</returns>
        public static TopologySortResults SortTopology(Edge[][] graph)
            => new TopologySort(graph).Sort();

        class TopologySort
        {
            private readonly Edge[][] _graph;
            private readonly NodeState[] _nodeStates;
            private readonly List<Edge> _route;
            private readonly Queue<Edge> _cycleRoute = new Queue<Edge>();

            public TopologySort(Edge[][] graph)
            {
                _graph = graph;
                _nodeStates = new NodeState[graph.Length];
                _route = new List<Edge>(graph.Length);
            }

            public TopologySortResults Sort()
            {
                for (int i = 0; i < _graph.Length; i++)
                {
                    if (!RecSort(new Edge(i, EdgeType.Root)))
                        return new TopologySortResults(_cycleRoute.ToArray(), null, true);
                }

                return new TopologySortResults(_route, null, false);
            }

            private bool RecSort(Edge edge, int from = -1)
            {
                var node = edge.To;
                if (_graph[node] == null)
                {
                    _nodeStates[node] = NodeState.Checked;
                    return true;
                }

                switch (_nodeStates[node])
                {
                    case NodeState.Checked: return true;
                    case NodeState.Checking: return false;
                    default:
                        _nodeStates[node] = NodeState.Checking;
                        for (int child = 0; child < _graph[node].Length; child++)
                        {
                            var to = _graph[node][child];
                            if (from == to.To && edge.Type== EdgeType.Equal)
                                continue; //reference. Skip route back

                            if (!RecSort(edge: to, from: node))
                            {
                                _cycleRoute.Enqueue(_graph[node][child]);
                                return false;
                            }
                        }

                        _nodeStates[node] = NodeState.Checked;

                        _route.Add(edge);
                        return true;
                }
            }
        }
    }

    public struct TopologySortResults
    {
        /// <summary>
        /// Topological sort order if has no cycle
        /// First cycle route otherwise
        /// </summary>
        public readonly IList<Edge> NodeNames;
        public readonly bool HasCycle;
        /// <summary>
        /// List of recursive nodes or null if there are no one
        /// </summary>
        public readonly int[] RecursionsOrNull;

        public TopologySortResults(IList<Edge> nodeNames, int[] recursionsOrNull, bool hasCycle)
        {
            NodeNames = nodeNames;
            HasCycle = hasCycle;
            RecursionsOrNull = recursionsOrNull;
        }
    }
}