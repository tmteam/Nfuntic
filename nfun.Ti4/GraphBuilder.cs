using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

            var res = new SolvingNode("T" + id) {NodeState = new ConstrainsSolvingState()};
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
                throw new InvalidOperationException(
                    $"Node {node} cannot be referenced by '{name}' because it is not constrained node.");
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
            else if (!left.IsSolved)
            {
                throw new NotImplementedException();
            }

            if (right.NodeState is ConstrainsSolvingState rconstrains)
            {
                rconstrains.DescedantTypes.Add(ConcreteType.U24);
                rconstrains.AncestorTypes.Add(ConcreteType.Real);
            }
            else if (!right.IsSolved)
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
            _nodes[id] = new ConcreteTypeSolvingNode(type.Name, "T" + id);
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
            int iteration = 0;
            while (true)
            {

                var allNodes = _nodes.Concat(_variables.Values).ToArray();
                if (iteration > allNodes.Length * allNodes.Length)
                    throw new InvalidOperationException();
                iteration++;

                var graph = new int[allNodes.Length][];
                for (int i = 0; i < allNodes.Length; i++)
                {
                    allNodes[i].GraphId = i;
                }

                for (int i = 0; i < allNodes.Length; i++)
                {
                    var node = allNodes[i];
                    var edges = node.Ancestors.Select(a => a.GraphId);
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
                var result = sorted.NodeNames.Select(n => allNodes[n]).Reverse().ToArray();
                if (sorted.HasCycle)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Found cycle: ");
                    Console.ResetColor();
                    Console.WriteLine(string.Join("->", result.Select(r => r.Name)));

                    //main node. every other node has to reference on it
                    MergeCycle(result);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Cycle normalization results: ");
                    Console.ResetColor();
                    foreach (var solvingNode in result)
                        solvingNode.PrintToConsole();
                }
                else
                {
                    foreach (var solvingNode in result)
                    {
                        if (solvingNode.NodeState is ConstrainsSolvingState constrains)
                        {
                            if (constrains.AncestorTypes.Count > 1)
                            {
                                var ancestor = ConcreteType.GetLastCommonAncestor(constrains.AncestorTypes);
                                constrains.AncestorTypes.Clear();
                                constrains.AncestorTypes.Add(ancestor);
                            }

                            if (constrains.DescedantTypes.Count>1)
                            {
                                var descedants = ConcreteType.GetFirstCommonDescendantOrNull(constrains.DescedantTypes);
                                constrains.DescedantTypes.Clear();
                                constrains.DescedantTypes.Add(descedants);
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
        }

        public static void UpdateUpwards(SolvingNode[] toposortedNodes)
        {
            foreach (var node in toposortedNodes)
            {
                foreach (var ancestor in node.Ancestors)
                {
                    ancestor.NodeState = MergeUpwardsStates(node, ancestor);
                }
            }
        }

        private static object MergeUpwardsStates(SolvingNode descendant, SolvingNode ancestor)
        {
            if (ancestor.NodeState is ReferenceSolvingState referenceAnc)
            {
                referenceAnc.RefTo.NodeState = MergeUpwardsStates(descendant, referenceAnc.RefTo);
                return referenceAnc;

            }
            if (descendant.NodeState is ConcreteType concreteDesc)
            {
                switch (ancestor.NodeState)
                {
                    case ConcreteType concreteAnc:
                    {
                        if (!concreteDesc.CanBeImplicitlyConvertedTo(concreteAnc))
                            throw new InvalidOperationException();
                        return ancestor.NodeState;
                    }
                    case ConstrainsSolvingState constrainsAnc:
                    {
                        var result = new ConstrainsSolvingState {PreferedType = constrainsAnc.PreferedType};
                        result.AncestorTypes.AddRange(
                            constrainsAnc.AncestorTypes);
                        result.DescedantTypes.Add(
                            ConcreteType.GetLastCommonAncestor(constrainsAnc.DescedantTypes.Append(concreteDesc)));
                        result.Validate();
                        return result;
                    }
                    default:
                        throw new NotSupportedException();
                }
            }
            else if (descendant.NodeState is ConstrainsSolvingState constrainsDesc)
            {
                switch (ancestor.NodeState)
                {
                    case ConcreteType concreteAnc:
                    {
                        if(constrainsDesc.DescedantTypes.Any() 
                           &&  ConcreteType.GetLastCommonAncestor(constrainsDesc.DescedantTypes)?.CanBeImplicitlyConvertedTo(concreteAnc) != true)
                            throw new InvalidOperationException();
                        return ancestor.NodeState;
                    }
                    case ConstrainsSolvingState constrainsAnc:
                    {
                        var result = new ConstrainsSolvingState { PreferedType = constrainsAnc.PreferedType };
                        result.AncestorTypes.AddRange(constrainsAnc.AncestorTypes);
                        result.DescedantTypes.Add(ConcreteType.GetLastCommonAncestor(constrainsAnc.DescedantTypes.Concat(constrainsDesc.DescedantTypes)));
                        result.Validate();
                        return result;
                    }
                    default:
                        throw new NotSupportedException();
                }
            }
            throw new NotSupportedException();
        }


        public void PrintTrace()
        {
            foreach (var solvingNode in _nodes)
            {
                if (solvingNode == null)
                    continue;

                solvingNode.PrintToConsole();
            }

            foreach (var solvingNode in _variables)
            {
                solvingNode.Value.PrintToConsole();
            }
        }

        private static void MergeCycle(SolvingNode[] cycleRoute)
        {
            var main = cycleRoute.FirstOrDefault(r => r.Type == SolvingNodeType.Variable) ?? cycleRoute.First();
            foreach (var current in cycleRoute)
            {
                if(current==main)
                    continue;
                
                if (current.NodeState is ReferenceSolvingState refState)
                {
                    if (!cycleRoute.Contains(refState.RefTo))
                        throw new NotImplementedException();
                }
                else
                {
                    //merge main and current
                    main.Ancestors.AddRange(current.Ancestors);
                    if (main.NodeState is ConcreteType concrete)
                    {
                        if (current.NodeState is ConcreteType concreteB)
                            main.NodeState = SolvingStateMergeFunctions.Merge(concrete, concreteB);
                        else if (current.NodeState is ConstrainsSolvingState constrainsB)
                            main.NodeState = SolvingStateMergeFunctions.Merge(constrainsB, concrete);
                        else throw new NotImplementedException();
                    }
                    else if (main.NodeState is ConstrainsSolvingState constrainsA)
                    {
                        if (current.NodeState is ConcreteType concreteB)
                            main.NodeState = SolvingStateMergeFunctions.Merge(constrainsA, concreteB);
                        else if (current.NodeState is ConstrainsSolvingState constrainsB)
                            main.NodeState = SolvingStateMergeFunctions.Merge(constrainsB, constrainsA);
                        else throw new NotImplementedException();
                    }
                    else throw new NotImplementedException();
                }
                current.NodeState = new ReferenceSolvingState() { RefTo = main };
            }

            var newAncestors = cycleRoute
                .SelectMany(r => r.Ancestors)
                .Where(r => !cycleRoute.Contains(r))
                .Distinct()
                .ToList();

            main.Ancestors.Clear();
            main.Ancestors.AddRange(newAncestors);
        }


    }
}
