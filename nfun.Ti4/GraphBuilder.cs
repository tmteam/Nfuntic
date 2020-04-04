using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace nfun.Ti4
{
    public class GraphBuilder
    {
        private readonly Dictionary<string, SolvingNode> _variables = new Dictionary<string, SolvingNode>();
        private readonly List<SolvingNode> _syntaxNodes = new List<SolvingNode>();
        private readonly List<SolvingNode> typeVariables = new List<SolvingNode>();

        private int varNodeId = 0;
        private SolvingNode GetNamedNode(string name)
        {
            if (_variables.TryGetValue(name, out var varnode))
            {
                return varnode;
            }

            var ans = new SolvingNode("T" + name)
            {
                NodeState = new SolvingConstrains(),
                Type =  SolvingNodeType.Named,
            };
            _variables.Add(name, ans);
            return ans;
        }

        private SolvingNode GetOrCreateNode(int id)
        {
            while (_syntaxNodes.Count <= id)
            {
                _syntaxNodes.Add(null);
            }

            var alreadyExists = _syntaxNodes[id];
            if (alreadyExists != null)
                return alreadyExists;

            var res = new SolvingNode("T" + id) {NodeState = new SolvingConstrains()};
            _syntaxNodes[id] = res;
            return res;
        }

        public void SetVar(string name, int node)
        {
            var namedNode = GetNamedNode(name);
            namedNode.Type = SolvingNodeType.Named;
            var idNode = GetOrCreateNode(node);
            if (idNode.NodeState is SolvingConstrains constrains)
            {
                namedNode.Ancestors.Add(idNode);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Node {node} cannot be referenced by '{name}' because it is not constrained node.");
            }
        }

        public void SetBitShift(int leftId, int rightId, int resultId)
        {
            var left = GetOrCreateNode(leftId);
            var right = GetOrCreateNode(rightId);
            var result = GetOrCreateNode(resultId);

            if (right.NodeState is SolvingConstrains constrainsR)
                right.NodeState = SolvingFunctions.Merge(constrainsR, ConcreteType.I32);

            
            if (result.NodeState is SolvingConstrains constrains)
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

            if (left.NodeState is SolvingConstrains lconstrains)
            {
                lconstrains.DescedantTypes.Add(ConcreteType.U24);
                lconstrains.AncestorTypes.Add(ConcreteType.Real);
            }
            else if (!left.IsSolved)
            {
                throw new NotImplementedException();
            }
        }
        public void SetArith(int leftId, int rightId, int resultId)
        {
            var left = GetOrCreateNode(leftId);
            var right = GetOrCreateNode(rightId);
            var result = GetOrCreateNode(resultId);
            if (result.NodeState is SolvingConstrains constrains)
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

            if (left.NodeState is SolvingConstrains lconstrains)
            {
                lconstrains.DescedantTypes.Add(ConcreteType.U24);
                lconstrains.AncestorTypes.Add(ConcreteType.Real);
            }
            else if (!left.IsSolved)
            {
                throw new NotImplementedException();
            }

            if (right.NodeState is SolvingConstrains rconstrains)
            {
                rconstrains.DescedantTypes.Add(ConcreteType.U24);
                rconstrains.AncestorTypes.Add(ConcreteType.Real);
            }
            else if (!right.IsSolved)
            {
                throw new NotImplementedException();
            }
        }

        public void SetArith2(int leftId, int rightId, int resultId)
        {
            var left   = GetOrCreateNode(leftId);
            var right  = GetOrCreateNode(rightId);
            var result = GetOrCreateNode(resultId);

            var varNode = new SolvingNode("V" + varNodeId)
            {
                Type = SolvingNodeType.TypeVariable
            };
            var constrains = new SolvingConstrains();
            varNode.NodeState = constrains;

            constrains.DescedantTypes.Add(ConcreteType.U24);
            constrains.AncestorTypes.Add(ConcreteType.Real);

            varNode.BecomeReferenceFor(result);
            varNode.BecomeAncestorFor(left);
            varNode.BecomeAncestorFor(right);

            typeVariables.Add(varNode);
            
            varNodeId++;
        }

        public void SetIfElse( int[] conditions, int[] expressions, int elseId, int resultId)
        {
            var result = GetOrCreateNode(resultId);
            throw new NotImplementedException();
        }

        public void SetEquality(int leftId, int rightId, int resultId)
        {
            var left = GetOrCreateNode(leftId);
            var right = GetOrCreateNode(rightId);
            var result = GetOrCreateNode(resultId);

            result.NodeState = ConcreteType.Bool;
            if(!SolvingFunctions.SetEqual(left, right))
                throw new InvalidOperationException();
        }

        public void SetComparable(int leftId, int rightId, int resultId)
        {
            var left = GetOrCreateNode(leftId);
            var right = GetOrCreateNode(rightId);
            var result = GetOrCreateNode(resultId);

            result.NodeState = ConcreteType.Bool;

            var leftState = left.GetNonReference().NodeState;
            if (leftState is SolvingConstrains constrains)
            {
                constrains.IsComparable = true;
            }
            else if (leftState is ConcreteType concrete)
            {
                if(!concrete.IsComparable)
                    throw new InvalidOperationException();
            }

            var rightState = right.GetNonReference().NodeState;
            if (rightState is SolvingConstrains constrainsR)
            {
                constrainsR.IsComparable = true;
            }
            else if (rightState is ConcreteType concreteR)
            {
                if (!concreteR.IsComparable)
                    throw new InvalidOperationException();
            }

            if (!SolvingFunctions.SetEqual(left, right))
                throw new InvalidOperationException();
        }

        public void SetConst(int id, ConcreteType type)
        {
            while (_syntaxNodes.Count <= id)
            {
                _syntaxNodes.Add(null);
            }

            var alreadyExists = _syntaxNodes[id];
            if (alreadyExists != null)
                throw new NotImplementedException();
            _syntaxNodes[id] = new ConcreteTypeSolvingNode(type.Name, "T" + id);
        }

        public void SetIntConst(int id, ConcreteType desc)
        {
            var node = GetOrCreateNode(id);
            if (node.NodeState is SolvingConstrains constrains)
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
            var defNode = GetNamedNode(name);

            if (exprNode.IsSolved)
                defNode.NodeState = exprNode.NodeState;
            else if (defNode.NodeState is SolvingConstrains constrains)
                exprNode.Ancestors.Add(defNode);
            else
                throw new NotImplementedException();
        }

        public SolvingNode[] Toposort()
        {
            int iteration = 0;
            while (true)
            {

                var allNodes = _syntaxNodes.Concat(_variables.Values).Concat(typeVariables).ToArray();
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
                    if (node.NodeState is RefTo reference)
                    {
                        //todo 2side reference
                        todo
                        graph[i] = edges.Append(reference.Node.GraphId).ToArray();
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
                        if (solvingNode.NodeState is SolvingConstrains constrains)
                        {
                            if (constrains.AncestorTypes.Count > 1)
                            {
                                var ancestor = constrains.AncestorTypes.GetCommonAncestor();
                                constrains.AncestorTypes.Clear();
                                constrains.AncestorTypes.Add(ancestor);
                            }

                            if (constrains.DescedantTypes.Count>1)
                            {
                                var descedants = constrains.DescedantTypes.GetCommonDescendantOrNull();
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


        public void PrintTrace()
        {
            foreach (var solvingNode in _syntaxNodes)
            {
                solvingNode?.PrintToConsole();
            }

            foreach (var solvingNode in _variables)
            {
                solvingNode.Value.PrintToConsole();
            }

            foreach (var typeVariable in typeVariables)
            {
                typeVariable.PrintToConsole();
            }
        }

        private static void MergeCycle(SolvingNode[] cycleRoute)
        {
            var main = cycleRoute.FirstOrDefault(r => r.Type == SolvingNodeType.Named) ?? cycleRoute.First();
            foreach (var current in cycleRoute)
            {
                if(current==main)
                    continue;
                
                if (current.NodeState is RefTo refState)
                {
                    if (!cycleRoute.Contains(refState.Node))
                        throw new NotImplementedException();
                }
                else
                {
                    //merge main and current
                    main.Ancestors.AddRange(current.Ancestors);
                    if (main.NodeState is ConcreteType concrete)
                    {
                        if (current.NodeState is ConcreteType concreteB)
                            main.NodeState = SolvingFunctions.Merge(concrete, concreteB);
                        else if (current.NodeState is SolvingConstrains constrainsB)
                            main.NodeState = SolvingFunctions.Merge(constrainsB, concrete);
                        else throw new NotImplementedException();
                    }
                    else if (main.NodeState is SolvingConstrains constrainsA)
                    {
                        if (current.NodeState is ConcreteType concreteB)
                            main.NodeState = SolvingFunctions.Merge(constrainsA, concreteB);
                        else if (current.NodeState is SolvingConstrains constrainsB)
                            main.NodeState = SolvingFunctions.Merge(constrainsB, constrainsA);
                        else throw new NotImplementedException();
                    }
                    else throw new NotImplementedException();
                }
                current.NodeState = new RefTo(main);
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
