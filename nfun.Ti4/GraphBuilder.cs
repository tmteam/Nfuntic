﻿using System;
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
        private readonly List<SolvingNode> _typeVariables = new List<SolvingNode>();
        private int _varNodeId = 0;
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

        private SolvingNode SetOrCreateConcrete(int id, ConcreteType type)
        {
            var node = GetOrCreateNode(id);
            if(!node.BecomeConcrete(type))
                throw new InvalidOperationException();
            return node;
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

            var res = new SolvingNode(id.ToString()) {NodeState = new SolvingConstrains()};
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

        public void SetBitwise(int leftId, int rightId, int resultId)
        {
            var left    = GetOrCreateNode(leftId);
            var right   = GetOrCreateNode(rightId);
            var result  = GetOrCreateNode(resultId);
            
            var varNode = CreateVarType(new SolvingConstrains(ConcreteType.U8, ConcreteType.I96));

            varNode.BecomeReferenceFor(result);
            varNode.BecomeAncestorFor(left);
            varNode.BecomeAncestorFor(right);
        }

        public void SetBitShift(int leftId, int rightId, int resultId)
        {
            var left    = GetOrCreateNode(leftId);
            SetOrCreateConcrete(rightId, ConcreteType.I48);
            var result  = GetOrCreateNode(resultId);

            var varNode = CreateVarType(new SolvingConstrains(ConcreteType.U24, ConcreteType.I96));

            varNode.BecomeReferenceFor(result);
            varNode.BecomeAncestorFor(left);
        }

        public void SetArith(int leftId, int rightId, int resultId)
        {
            var left   = GetOrCreateNode(leftId);
            var right  = GetOrCreateNode(rightId);
            var result = GetOrCreateNode(resultId);

            var varNode = CreateVarType(new SolvingConstrains(ConcreteType.U24, ConcreteType.Real));

            varNode.BecomeReferenceFor(result);

            varNode.BecomeAncestorFor(left);
            varNode.BecomeAncestorFor(right);
        }
        
        public void SetIfElse( int[] conditions, int[] expressions, int resultId)
        {
            var result = GetOrCreateNode(resultId);
            foreach (var exprId in expressions)
            {
                var expr = GetOrCreateNode(exprId);
                result.BecomeReferenceFor(expr);
            }

            foreach (var condId in conditions)
                SetOrCreateConcrete(condId, ConcreteType.Bool);
        }

        public void SetEquality(int leftId, int rightId, int resultId)
        {
            var left = GetOrCreateNode(leftId);
            var right = GetOrCreateNode(rightId);
            SetOrCreateConcrete(resultId, ConcreteType.Bool);

            var varNode = CreateVarType();
            varNode.BecomeAncestorFor(left);
            varNode.BecomeAncestorFor(right);
        }

        public void SetComparable(int leftId, int rightId, int resultId)
        {
            var left   = GetOrCreateNode(leftId);
            var right  = GetOrCreateNode(rightId);
            SetOrCreateConcrete(resultId, ConcreteType.Bool);

            var varNode = CreateVarType(new SolvingConstrains(){ IsComparable = true});
            varNode.BecomeAncestorFor(left);
            varNode.BecomeAncestorFor(right);
        }

        public void SetConst(int id, ConcreteType type) 
            => SetOrCreateConcrete(id, type);

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
                defNode.BecomeReferenceFor(exprNode);
            else
                defNode.BecomeAncestorFor(exprNode);
        }

        public SolvingNode[] Toposort()
        {
            int iteration = 0;
            while (true)
            {

                var allNodes = _syntaxNodes.Concat(_variables.Values).Concat(_typeVariables).ToArray();
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
                solvingNode?.PrintToConsole();

            foreach (var solvingNode in _variables) 
                solvingNode.Value.PrintToConsole();

            foreach (var typeVariable in _typeVariables) 
                typeVariable.PrintToConsole();
        }

        private SolvingNode CreateVarType(object state = null)
        {
            var varNode = new SolvingNode("V" + _varNodeId)
            {
                Type = SolvingNodeType.TypeVariable
            };
            varNode.NodeState = state ?? new SolvingConstrains();
            _varNodeId++;
            _typeVariables.Add(varNode);

            return varNode;
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


        public FinalizationResults Solve()
        {
            PrintTrace();
            Console.WriteLine();

            var sorted = Toposort();

            Console.WriteLine("Decycled:");
            PrintTrace();

            Console.WriteLine();
            Console.WriteLine("Set up");

            SolvingFunctions.SetUpwardsLimits(sorted);
            PrintTrace();

            Console.WriteLine();
            Console.WriteLine("Set down");

            SolvingFunctions.SetDownwardsLimits(sorted);
            PrintTrace();

            SolvingFunctions.Destruction(sorted);

            Console.WriteLine();
            Console.WriteLine("Destruct Down");
            PrintTrace();

            Console.WriteLine("Finalize");
            var results = SolvingFunctions.FinalizeUp(sorted);

            Console.WriteLine($"Type variables: {results.TypeVariables.Length}");
            foreach (var typeVariable in results.TypeVariables)
                Console.WriteLine("    " + typeVariable);

            Console.WriteLine($"Syntax node types: ");
            foreach (var syntaxNode in results.SyntaxNodes.Where(s => s != null))
                Console.WriteLine("    " + syntaxNode);

            Console.WriteLine($"Named node types: ");
            foreach (var namedNode in results.NamedNodes)
                Console.WriteLine("    " + namedNode);

            return results;
        }

        public void SetVarType(string s, ConcreteType u64)
        {
            var node = GetNamedNode(s);
            if(!node.BecomeConcrete(u64))
                throw new InvalidOperationException();
        }

        public void SetCall(ConcreteType typesOfTheCall, params int[] argumentsThenResult)
        {
            for (int i = 0; i < argumentsThenResult.Length-1; i++)
            {
                var argId = argumentsThenResult[i];
                var node = GetOrCreateNode(argId);
                node.SetAncestor(typesOfTheCall);
            }

            var resultId = argumentsThenResult[argumentsThenResult.Length - 1];
            var resultNode = GetOrCreateNode(resultId);
            resultNode.BecomeConcrete(typesOfTheCall);
        }
    }
}
