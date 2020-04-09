using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace nfun.Ti4
{

    public class ConcreteTypeSolvingNode : SolvingNode
    {
        public ConcreteTypeSolvingNode(PrimitiveTypeName typeName, string nodeName) : base(nodeName)
        {
            this.NodeState = new ConcreteType(typeName);
        }
    }

    public enum SolvingNodeType
    {
        Named,
        SyntaxNode,
        TypeVariable
    }

    public class SolvingNode
    {
        public int GraphId { get; set; }

        public SolvingNode(string name)
        {
            Name = name;
        }

        public SolvingNodeType Type { get; set; } = SolvingNodeType.SyntaxNode;
        public List<SolvingNode> Ancestors { get; } = new List<SolvingNode>();

        public bool IsSolved => NodeState is ConcreteType;
        public object NodeState { get; set; }

        public string Name { get; }
        public override string ToString() => Name + "." + NodeState;

        public void PrintToConsole()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{Name}:");
            Console.ResetColor();

            if (NodeState is ConcreteType concrete)
                Console.WriteLine($"{concrete.Name} ");
            else if (NodeState is RefTo reference)
            {
                if(Ancestors.Any())
                    Console.WriteLine($"{reference.Node.Name} <={string.Join(",", Ancestors)}");
                else
                    Console.WriteLine($"{reference.Node.Name} ");
            }
            else if (NodeState is SolvingConstrains constrains)
            {
                Console.Write($"[ ");

                if (constrains.HasDescendant)
                    Console.Write(constrains.Descedant);

                Console.Write(" .. ");

                if (constrains.HasAncestor || Ancestors.Any())
                {
                    if (constrains.HasAncestor)
                        Console.Write($"{constrains.Ancestor}, ({(string.Join(", ", Ancestors))})");
                    else
                        Console.Write($"({(string.Join(", ", Ancestors))})");
                }

                Console.Write(" ]");
                if(constrains.IsComparable)
                    Console.Write(" <>");
                if (constrains.PreferedType != null)
                    Console.Write($" Pref: {constrains.PreferedType.Name}");
                Console.WriteLine();
            }
        }


        public bool BecomeConcrete(ConcreteType concrete)
        {
            if (this.NodeState is ConcreteType oldConcrete)
            {
                return oldConcrete == concrete;
            }
            else if (this.NodeState is SolvingConstrains constrains)
            {
                if (!constrains.Fits(concrete))
                    return false;
                this.NodeState = concrete;
                return true;
            }

            return false;
        }

        public void SetAncestor(ConcreteType anc)
        {
            var node = this;
            if (node.NodeState is RefTo)
                node = node.GetNonReference();

            if(node.NodeState is ConcreteType oldConcrete)
            {
                if(!oldConcrete.CanBeImplicitlyConvertedTo(anc))
                    throw new InvalidOperationException();
            }
            else if (node.NodeState is SolvingConstrains constrains)
            {
                constrains.AddAncestor(anc);
            };
        }
    }

    public class RefTo
    {
        public RefTo(SolvingNode node)
        {
            if (node.Type != SolvingNodeType.TypeVariable)
            {

            }
            Node = node;
        }
        public SolvingNode Node { get; }
        public override string ToString() => "RefTo." + Node;
    }
}
