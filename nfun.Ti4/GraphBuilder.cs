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
                constrains.Ancestors.Add(varnode);
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
                constrains.Descedants.Add(left);
                constrains.Descedants.Add(right);
                constrains.Descedants.Add(SolvingNode.U24);
                constrains.Ancestors.Add(SolvingNode.Real);
            }
            else
            {
                throw new NotImplementedException();
            }
            
            if (left.NodeState is ConstrainsSolvingState lconstrains)
            {
                lconstrains.Descedants.Add(SolvingNode.U24);
                lconstrains.Ancestors.Add(SolvingNode.Real);
            }
            else if(!left.IsSolved)
            {
                throw new NotImplementedException();
            }

            if (right.NodeState is ConstrainsSolvingState rconstrains)
            {
                rconstrains.Descedants.Add(SolvingNode.U24);
                rconstrains.Ancestors.Add(SolvingNode.Real);
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

        public void SetIntConst(int id, SolvingNode desc)
        {
            var node = GetOrCreateNode(id);
            if (node.NodeState is ConstrainsSolvingState constrains)
            {
                constrains.Ancestors.Add(SolvingNode.Real);
                constrains.Descedants.Add(desc);
                constrains.PreferedType = ConcreteType.Real;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void SetDef(string name, int rightNodeId)
        {
            var node = GetOrCreateNode(rightNodeId);
            var varnode = GetVarNode(name);

            if (node.IsSolved)
                varnode.NodeState = node.NodeState;
            else
                if (varnode.NodeState is ConstrainsSolvingState constrains)
                {
                    constrains.Descedants.Add(node);
                }
            else throw new NotImplementedException();
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

                if (constrains.Descedants.Count == 1)
                    Console.Write(constrains.Descedants.First().Name);
                else if (constrains.Descedants.Any())
                    Console.Write($"({string.Join(", ", constrains.Descedants.Select(a => a.Name))})");

                Console.Write(" .. ");

                if (constrains.Ancestors.Count == 1)
                    Console.Write(constrains.Ancestors.First().Name);
                else if (constrains.Ancestors.Any())
                    Console.Write($"({(string.Join(", ", constrains.Ancestors.Select(a => a.Name)))})");


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
