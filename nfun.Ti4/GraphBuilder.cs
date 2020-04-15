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
        private readonly List<SolvingNode> _typeVariables = new List<SolvingNode>();
        private int _varNodeId = 0;
        private SolvingNode GetNamedNode(string name)
        {
            if (_variables.TryGetValue(name, out var varnode))
            {
                return varnode;
            }

            var ans = new SolvingNode("T" + name, new SolvingConstrains(), SolvingNodeType.Named);
            _variables.Add(name, ans);
            return ans;
        }

        public void SetVar(string name, int node)
        {
            var namedNode = GetNamedNode(name);
            var idNode = GetOrCreateNode(node);
            if (idNode.State is SolvingConstrains)
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
                SetOrCreateConcrete(condId, PrimitiveType.Bool);
        }

        public void SetEquality(int leftId, int rightId, int resultId)
        {
            var left = GetOrCreateNode(leftId);
            var right = GetOrCreateNode(rightId);
            SetOrCreateConcrete(resultId, PrimitiveType.Bool);

            var varNode = CreateVarType();
            varNode.BecomeAncestorFor(left);
            varNode.BecomeAncestorFor(right);
        }

        public void SetComparable(int leftId, int rightId, int resultId)
        {
            var left   = GetOrCreateNode(leftId);
            var right  = GetOrCreateNode(rightId);
            SetOrCreateConcrete(resultId, PrimitiveType.Bool);

            var varNode = CreateVarType(new SolvingConstrains(){ IsComparable = true});
            varNode.BecomeAncestorFor(left);
            varNode.BecomeAncestorFor(right);
        }

        public void SetConst(int id, PrimitiveType type) 
            => SetOrCreateConcrete(id, type);

        public void SetIntConst(int id, PrimitiveType desc)
        {
            var node = GetOrCreateNode(id);
            if (node.State is SolvingConstrains constrains)
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

        public void SetVarType(string s, ArrayOf array)
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

        public void SetCall(PrimitiveType[] argThenReturnTypes, int[] argThenReturnIds)
        {
            for (int i = 0; i < argThenReturnIds.Length - 1; i++)
            {
                var node = GetOrCreateNode(argThenReturnIds[i]);
                node.SetAncestor(argThenReturnTypes[i]);
            }
            SetOrCreateConcrete(
                argThenReturnIds[argThenReturnIds.Length - 1],
                argThenReturnTypes[argThenReturnIds.Length - 1]);
        }
        public void SetCall(PrimitiveType typesOfTheCall, params int[] argumentsThenResult)
        {
            PrimitiveType[] types = new PrimitiveType[argumentsThenResult.Length];
            for (int i = 0; i < types.Length; i++)
            {
                types[i] = typesOfTheCall;
            };
            SetCall(types, argumentsThenResult);
            /*
            for (int i = 0; i < argumentsThenResult.Length; i++)
            {
                var argId = argumentsThenResult[i];
                SetOrCreateConcrete(argId, typesOfTheCall);
            }*/
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
                    var edges = node.Ancestors
                        .Union(node.MemberOf)
                        .Select(a => a.GraphId);
                    
                    if (node.State is RefTo reference)
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
        #region Calls

        public void SetBitwiseInvert(int argId, int resultId)
        {
            var arg = GetOrCreateNode(argId);
            var result = GetOrCreateNode(resultId);

            var varNode = CreateVarType(new SolvingConstrains(PrimitiveType.U8, PrimitiveType.I96));

            varNode.BecomeReferenceFor(result);
            varNode.BecomeAncestorFor(arg);
        }

        public void SetBitwise(int leftId, int rightId, int resultId)
        {
            var left = GetOrCreateNode(leftId);
            var right = GetOrCreateNode(rightId);
            var result = GetOrCreateNode(resultId);

            var varNode = CreateVarType(new SolvingConstrains(PrimitiveType.U8, PrimitiveType.I96));

            varNode.BecomeReferenceFor(result);
            varNode.BecomeAncestorFor(left);
            varNode.BecomeAncestorFor(right);
        }

        public void SetBitShift(int leftId, int rightId, int resultId)
        {
            var left = GetOrCreateNode(leftId);
            SetOrCreateConcrete(rightId, PrimitiveType.I48);
            var result = GetOrCreateNode(resultId);

            var varNode = CreateVarType(new SolvingConstrains(PrimitiveType.U24, PrimitiveType.I96));

            varNode.BecomeReferenceFor(result);
            varNode.BecomeAncestorFor(left);
        }

        public void SetArith(int leftId, int rightId, int resultId)
        {
            var left = GetOrCreateNode(leftId);
            var right = GetOrCreateNode(rightId);
            var result = GetOrCreateNode(resultId);

            var varNode = CreateVarType(new SolvingConstrains(PrimitiveType.U24, PrimitiveType.Real));

            varNode.BecomeReferenceFor(result);

            varNode.BecomeAncestorFor(left);
            varNode.BecomeAncestorFor(right);
        }


        public void SetNegateCall(int argId, int resultId)
        {
            var vartype = CreateVarType(new SolvingConstrains(PrimitiveType.I16, PrimitiveType.Real));
            var arg = GetOrCreateNode(argId);
            var res = GetOrCreateNode(resultId);
            vartype.BecomeReferenceFor(res);
            vartype.BecomeAncestorFor(arg);
        }

        public void SetArrGetCall(int arrArgId, int indexArgId, int resId)
        {
            var vartype = CreateVarType();
            GetOrCreateArrayNode(arrArgId, vartype);
            GetOrCreateNode(indexArgId).SetAncestor(PrimitiveType.I32);
            var result = GetOrCreateNode(resId);
            vartype.BecomeReferenceFor(result);
        }

        public void SetConcatCall(int firstId, int secondId, int resultId)
        {
            var vartype = CreateVarType();
            var arrType = CreateVarType(new ArrayOf(vartype));
            var first = GetOrCreateNode(firstId);
            arrType.BecomeAncestorFor(first);
            var second = GetOrCreateNode(secondId);
            arrType.BecomeAncestorFor(second);

            var result = GetOrCreateNode(resultId);
            result.State = new RefTo(arrType);
        }
        #endregion

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
                if(node.State is ArrayOf arr)
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

      

        private SolvingNode SetOrCreateConcrete(int id, PrimitiveType type)
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
                if (alreadyExists.State is SolvingConstrains constrains && constrains.NoConstrains)
                {
                    alreadyExists.State = new ArrayOf(elementType);
                    return alreadyExists;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            var res = new SolvingNode(id.ToString(), new ArrayOf(elementType), SolvingNodeType.SyntaxNode);
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

            var res = new SolvingNode(id.ToString(), new SolvingConstrains(), SolvingNodeType.SyntaxNode);
            _syntaxNodes[id] = res;
            return res;
        }

        private SolvingNode CreateVarType(object state = null)
        {
            var varNode = new SolvingNode(
                name:  "V" + _varNodeId,
                state: state ?? new SolvingConstrains(),
                type:  SolvingNodeType.TypeVariable);
            _varNodeId++;
            _typeVariables.Add(varNode);
            return varNode;
        }


    }
}
