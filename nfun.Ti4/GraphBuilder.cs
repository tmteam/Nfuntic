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

        public void SetConst(int id, SolvingNode type)
        {
            while (_nodes.Count < id)
            {
                _nodes.Add(null);
            }

            var alreadyExists = _nodes[id];
            if (alreadyExists != null)
                throw new NotImplementedException();
            _nodes[id] = type;
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

        private void PrintNode(SolvingNode solvingNode)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{solvingNode.Name}:");
            Console.ResetColor();

            if (solvingNode.NodeState is ConcreteTypeSolvingNode concrete)
            {
                Console.Write($"{concrete.Name} ");
            }

            else if (solvingNode.NodeState is ReferenceSolvingState reference)
            {
                Console.Write($"{reference.RefTo.Name} ");
            }

            else if (solvingNode.NodeState is ConstrainsSolvingState constrains)
            {
                Console.Write($"[ ");

                if (constrains.DescedantTypes.Count == 1)
                    Console.Write(constrains.DescedantTypes.First().Name);
                else if (constrains.DescedantTypes.Any())
                    Console.Write($"({string.Join(", ", constrains.DescedantTypes.Select(a => a.Name))})");

                Console.Write(" .. ");

                if (constrains.AncestorTypes.Any() || solvingNode.Ancestors.Any())
                {
                    var typeNames = constrains
                        .AncestorTypes
                        .Select(a => a.Name.ToString());
                    var ancestorNames = solvingNode.Ancestors.Select(a => a.Name);

                    Console.Write($"({(string.Join(", ",typeNames.Concat(ancestorNames)))})");
                }

                Console.Write(" ]");

                if (constrains.PreferedType != null)
                    Console.Write($" Pref: {constrains.PreferedType.Name}");
                Console.WriteLine();
            }

        }
        public void PrintTrace()
        {
            foreach (var solvingNode in _nodes)
            {
                if(solvingNode==null)
                    continue;
                
                PrintNode(solvingNode);
            }

            foreach (var solvingNode in _variables)
            {
                PrintNode(solvingNode.Value);
            }
        }
    }
}
