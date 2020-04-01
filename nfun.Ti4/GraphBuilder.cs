using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nfun.Ti4
{
    public class GraphBuilder
    {
        private Dictionary<string, SolvingNode> _variables = new Dictionary<string, SolvingNode>();
        private List<SolvingNode> _nodes = new List<SolvingNode>();

        private SolvingNode GetVarNode(string name)
        {
            if (_variables.TryGetValue(name, out var varnode))
            {
                return varnode;
            }

            var ans = new SolvingNode("T" + name) {NodeState = new ConstrainsSolvingState()};
            _variables.Add(name, ans);
            return ans;
        }

        private SolvingNode GetOrCreateNode(int id)
        {
            while (_nodes.Count <= id)
            {
                _nodes.Add(null);
            }

            var alreadyExists = _nodes[id];
            if (alreadyExists != null)
                return alreadyExists;

            var res = new SolvingNode("T"+id) { NodeState = new ConstrainsSolvingState() };
            _nodes[id] = res;
            return res;
        }
        public void SetVar(string name, int node)
        {
            var varnode = GetVarNode(name);
            varnode.Type = SolvingNodeType.Variable;
            var idNode = GetOrCreateNode(node);
            if (idNode.NodeState is ConstrainsSolvingState constrains)
            {
                varnode.Ancestors.Add(idNode);
                //
                //idNode.Ancestors.Add(varnode);
            }
            else
            {
                throw new InvalidOperationException($"Node {node} cannot be referenced by '{name}' because it is not constrained node.");
            }
        }

        public void SetArith(int leftId, int rightId, int resultId)
        {
            var left = GetOrCreateNode(leftId);
            var right = GetOrCreateNode(rightId);
            var result = GetOrCreateNode(resultId);
            if (result.NodeState is ConstrainsSolvingState constrains)
            {
                left.Ancestors.Add(result);
                right.Ancestors.Add(result);
                constrains.DescedantTypes.Add(ConcreteType.U24);
                constrains.AncestorTypes.Add(ConcreteType.Real);
            }
            else
            {
                throw new NotImplementedException();
            }
            
            if (left.NodeState is ConstrainsSolvingState lconstrains)
            {
                lconstrains.DescedantTypes.Add(ConcreteType.U24);
                lconstrains.AncestorTypes.Add(ConcreteType.Real);
            }
            else if(!left.IsSolved)
            {
                throw new NotImplementedException();
            }

            if (right.NodeState is ConstrainsSolvingState rconstrains)
            {
                rconstrains.DescedantTypes.Add(ConcreteType.U24);
                rconstrains.AncestorTypes.Add(ConcreteType.Real);
            }
            else if(!right.IsSolved)
            {
                throw new NotImplementedException();
            }
        }

        public void SetConst(int id, ConcreteType type)
        {
            while (_nodes.Count <= id)
            {
                _nodes.Add(null);
            }

            var alreadyExists = _nodes[id];
            if (alreadyExists != null)
                throw new NotImplementedException();
            _nodes[id] = new ConcreteTypeSolvingNode(type.Name, "T"+id);
        }

        public void SetIntConst(int id, ConcreteType desc)
        {
            var node = GetOrCreateNode(id);
            if (node.NodeState is ConstrainsSolvingState constrains)
            {
                constrains.AncestorTypes.Add(ConcreteType.Real);
                constrains.DescedantTypes.Add(desc);
                constrains.PreferedType = ConcreteType.Real;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void SetDef(string name, int rightNodeId)
        {
            var exprNode = GetOrCreateNode(rightNodeId);
            var defNode = GetVarNode(name);

            if (exprNode.IsSolved)
                defNode.NodeState = exprNode.NodeState;
            else if (defNode.NodeState is ConstrainsSolvingState constrains)
                exprNode.Ancestors.Add(defNode);
            else 
                throw new NotImplementedException();
        }

        public SolvingNode[] Toposort()
        {
            while (true)
            {
                
            var allNodes = _nodes.Concat(_variables.Values).ToArray();
            var graph = new int[allNodes.Length][];
            for (int i = 0; i < allNodes.Length; i++)
            {
                allNodes[i].GraphId = i;
            }

            for (int i = 0; i < allNodes.Length; i++)
            {
                var node = allNodes[i];
                var edges =  node.Ancestors.Select(a => a.GraphId);
                if (node.NodeState is ReferenceSolvingState reference)
                {
                    graph[i] = edges.Append(reference.RefTo.GraphId).ToArray();
                }
                else
                {
                    graph[i] = edges.ToArray();
                }
            }

            var sorted = GraphTools.SortTopology(graph);
            var result = sorted.NodeNames.Select(n => allNodes[n]).ToArray();

            if (sorted.HasCycle)
            {
                //main node. every other node has to reference on it
                var main = result.FirstOrDefault(r => r.Type == SolvingNodeType.Variable)??result.First();
                for (int i = 0; i < result.Length; i++)
                {
                    var nextNodeId = i + 1;
                    if (nextNodeId == result.Length) nextNodeId = 0;
                    var current = result[i];
                    var next = result[nextNodeId];
                    if (current.Ancestors.Contains(next))
                    {
                        current.Ancestors.Remove(next);
                        //merge!
                    }
                    else if (current.NodeState is ReferenceSolvingState)
                    {
                        current.NodeState = new ReferenceSolvingState{RefTo = main};
                    }
                }
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Toposort results: ");
            Console.ResetColor();
            Console.WriteLine(string.Join("->", result.Select(r => r.Name)));
            return result;
            }

        }

       
        public void PrintTrace()
        {
            foreach (var solvingNode in _nodes)
            {
                if(solvingNode==null)
                    continue;
                
                solvingNode.PrintToConsole();
            }

            foreach (var solvingNode in _variables)
            {
                solvingNode.Value.PrintToConsole();
            }
        }
    }
}
