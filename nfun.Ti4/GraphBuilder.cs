﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace nfun.Ti4
{
    public class GraphBuilder
    {
        private readonly Dictionary<string, SolvingNode> _variables = new Dictionary<string, SolvingNode>();
        private readonly List<SolvingNode> _syntaxNodes = new List<SolvingNode>();
        private readonly List<SolvingNode> _typeVariables = new List<SolvingNode>();
        private int _varNodeId = 0;

        public RefTo InitializeVarNode(IType desc = null, PrimitiveType anc = null, bool isComparable = false) 
            => new RefTo(CreateVarType(new Constrains(desc, anc){IsComparable =  isComparable}));

        #region set primitives

        public void SetVar(string name, int node)
        {
            var namedNode = GetNamedNode(name);
            var idNode = GetOrCreateNode(node);
            if (idNode.State is Constrains)
            {
                namedNode.Ancestors.Add(idNode);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Node {node} cannot be referenced by '{name}' because it is not constrained node.");
            }
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
                SetOrCreatePrimitive(condId, PrimitiveType.Bool);
        }


        public void SetConst(int id, PrimitiveType type) 
            => SetOrCreatePrimitive(id, type);

        public void SetIntConst(int id, PrimitiveType desc)
        {
            var node = GetOrCreateNode(id);
            if (node.State is Constrains constrains)
            {
                constrains.AddAncestor(PrimitiveType.Real);
                constrains.AddDescedant(desc);
                constrains.PreferedType = PrimitiveType.Real;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void SetVarType(string s, Array array)
        {
            var node = GetNamedNode(s);
            node.State = array;
        }
        public void SetVarType(string s, PrimitiveType u64)
        {
            var node = GetNamedNode(s);
            if (!node.BecomeConcrete(u64))
                throw new InvalidOperationException();
        }

        public void CreateLabda(int returnId, int lambdaId, string[] varNames)
        {
            var arg = GetNamedNode(varNames[0]);
            var ret = GetOrCreateNode(returnId);
            SetOrCreateLambda(lambdaId, ret, arg);
        }

        


        public void SetArrayInit(int resultIds, params int[] elementIds)
        {
            var elementType = CreateVarType();
            var resultNode = GetOrCreateArrayNode(resultIds, elementType);

            foreach (var id in elementIds)
            {
                elementType.BecomeReferenceFor(GetOrCreateNode(id));
                elementType.MemberOf.Add(resultNode);
            }
        }

        public void SetCall(IState[] argThenReturnTypes, int[] argThenReturnIds)
        {
            if(argThenReturnTypes.Length!=argThenReturnIds.Length)
                throw new ArgumentException("Sizes of type and id array have to be equal");

            for (int i = 0; i < argThenReturnIds.Length - 1; i++)
            {
                var type = argThenReturnTypes[i];
                var argId = argThenReturnIds[i];
                switch (type)
                {
                    case PrimitiveType primitive:
                    {
                        var node = GetOrCreateNode(argId);
                        node.SetAncestor(primitive);
                        break;
                    }
                    case Array array:
                    {
                        GetOrCreateArrayNode(argId, array.ElementNode);
                        break;
                    }
                    case RefTo refTo:
                    {
                        var node = GetOrCreateNode(argId);
                        refTo.Node.BecomeAncestorFor(node);
                        break;
                    }
                    default: throw new InvalidOperationException();
                }
            }

            var returnId = argThenReturnIds[argThenReturnIds.Length - 1];
            var returnType = argThenReturnTypes[argThenReturnIds.Length - 1];
            var returnNode = GetOrCreateNode(returnId);
            returnNode.State =  SolvingFunctions.GetMergedState(returnNode.State, returnType);
        }
        public void SetDef(string name, int rightNodeId)
        {
            var exprNode = GetOrCreateNode(rightNodeId);
            var defNode = GetNamedNode(name);
                //todo use prefered type
           // if (exprNode.IsSolved)
           //    defNode.BecomeReferenceFor(exprNode);
           //else
                defNode.BecomeAncestorFor(exprNode);
        }
        #endregion
        public SolvingNode[] Toposort()
        {
            int iteration = 0;
            while (true)
            {

                var allNodes = _syntaxNodes.Concat(_variables.Values).Concat(_typeVariables).ToArray();
                if (iteration > allNodes.Length * allNodes.Length)
                    throw new InvalidOperationException();
                iteration++;

                var graph = ConvertToArrayGraph(allNodes);

                var sorted = GraphTools.SortTopology(graph);
                var result = sorted.NodeNames.Select(n => allNodes[n]).Reverse().ToArray();
                if (sorted.HasCycle)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Found cycle: ");
                    Console.ResetColor();
                    Console.WriteLine(string.Join("->", result.Select(r => r.Name)));

                    //main node. every other node has to reference on it
                    SolvingFunctions.MergeCycle(result);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Cycle normalization results: ");
                    Console.ResetColor();
                    foreach (var solvingNode in result)
                        solvingNode.PrintToConsole();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Toposort results: ");
                    Console.ResetColor();
                    Console.WriteLine(string.Join("->", result.Select(r => r.Name)));
                    return result;
                }
            }
        }

        private static int[][] ConvertToArrayGraph(SolvingNode[] allNodes)
        {
            var graph = new LinkedList<int>[allNodes.Length];
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
                    PutEdges(i,node);
                }
            }
            
            return graph.Select(g=>g?.ToArray()).ToArray();

            void PutEdges(int targetNode, SolvingNode source)
            {
                if(graph[targetNode]==null)
                    graph[targetNode] = new LinkedList<int>();
                foreach (var anc in source.Ancestors)
                {
                    graph[targetNode].AddLast(anc.GraphId);
                }
                if(source.State is RefTo reference)
                {
                    graph[targetNode].AddLast(reference.Node.GraphId);
                }
            }

        }


        public void PrintTrace()
        {
            var alreadyPrinted = new HashSet<SolvingNode>();

            var allNodes = _syntaxNodes.Union(_variables.Select(v => v.Value)).Union(_typeVariables);

            void ReqPrintNode(SolvingNode node)
            {
                if(node==null)
                    return;
                if(alreadyPrinted.Contains(node))
                    return;
                if(node.State is Array arr)
                    ReqPrintNode(arr.ElementNode);
                node.PrintToConsole();
                alreadyPrinted.Add(node);
            }

            foreach (var node in allNodes)
                ReqPrintNode(node);
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

        private SolvingNode GetNamedNode(string name)
        {
            if (_variables.TryGetValue(name, out var varnode))
            {
                return varnode;
            }

            var ans = new SolvingNode("T" + name, new Constrains(), SolvingNodeType.Named);
            _variables.Add(name, ans);
            return ans;
        }

        private void SetOrCreateLambda(int lambdaId, SolvingNode ret, SolvingNode arg)
        {
            var fun = Fun.Of(argNode: arg, returnNode: ret);

            while (_syntaxNodes.Count <= lambdaId)
            {
                _syntaxNodes.Add(null);
            }

            var alreadyExists = _syntaxNodes[lambdaId];
            if (alreadyExists != null)
            {
                alreadyExists.State = SolvingFunctions.GetMergedState(fun, alreadyExists.State);
            }
            else
            {
                var res = new SolvingNode(lambdaId.ToString(), fun, SolvingNodeType.SyntaxNode);
                _syntaxNodes[lambdaId] = res;
            }
        }
        private SolvingNode SetOrCreatePrimitive(int id, PrimitiveType type)
        {
            var node = GetOrCreateNode(id);
            if (!node.BecomeConcrete(type))
                throw new InvalidOperationException();
            return node;
        }

        private SolvingNode GetOrCreateArrayNode(int id, SolvingNode elementType)
        {
            while (_syntaxNodes.Count <= id)
            {
                _syntaxNodes.Add(null);
            }

            var alreadyExists = _syntaxNodes[id];
            if (alreadyExists != null)
            {
                alreadyExists.State = SolvingFunctions.GetMergedState(new Array(elementType), alreadyExists.State);
                return alreadyExists;
            }

            var res = new SolvingNode(id.ToString(), new Array(elementType), SolvingNodeType.SyntaxNode);
            _syntaxNodes[id] = res;
            return res;
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

            var res = new SolvingNode(id.ToString(), new Constrains(), SolvingNodeType.SyntaxNode);
            _syntaxNodes[id] = res;
            return res;
        }

        private SolvingNode CreateVarType(IState state = null)
        {
            var varNode = new SolvingNode(
                name:  "V" + _varNodeId,
                state: state ?? new Constrains(),
                type:  SolvingNodeType.TypeVariable);
            _varNodeId++;
            _typeVariables.Add(varNode);
            return varNode;
        }


    }
}
