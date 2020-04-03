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
        Variable,
        Node
    }

    public class SolvingNode
    {
        public int GraphId { get; set; }

        public SolvingNode(string name)
        {
            Name = name;
        }

        public SolvingNodeType Type { get; set; } = SolvingNodeType.Node;
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
                Console.WriteLine($"{reference.Node.Name} ");
            else if (NodeState is SolvingConstrains constrains)
            {
                Console.Write($"[ ");

                if (constrains.DescedantTypes.Count == 1)
                    Console.Write(constrains.DescedantTypes.First().Name);
                else if (constrains.DescedantTypes.Any())
                    Console.Write($"({string.Join(", ", constrains.DescedantTypes.Select(a => a.Name))})");

                Console.Write(" .. ");

                if (constrains.AncestorTypes.Any() || Ancestors.Any())
                {
                    var typeNames = constrains
                        .AncestorTypes
                        .Select(a => a.Name.ToString());
                    var ancestorNames = Ancestors.Select(a => a.Name);

                    Console.Write($"({(string.Join(", ", typeNames.Concat(ancestorNames)))})");
                }

                Console.Write(" ]");

                if (constrains.PreferedType != null)
                    Console.Write($" Pref: {constrains.PreferedType.Name}");
                Console.WriteLine();
            }
        }


    }

    public class RefTo
    {
        public RefTo(SolvingNode node)
        {
            Node = node;
        }
        public SolvingNode Node { get; }
        public override string ToString() => "RefTo." + Node;
    }

    public class SolvingConstrains
    {
        public bool Fits(ConcreteType concrete)
        {
            if (AncestorTypes.Any())
            {
                var anc = AncestorTypes.GetCommonDescendantOrNull();
                if (anc == null)
                    return false;
                if (!concrete.CanBeImplicitlyConvertedTo(anc))
                    return false;
            }

            if (DescedantTypes.Any())
            {
                var desc = DescedantTypes.GetCommonAncestor();
                if (!desc.CanBeImplicitlyConvertedTo(concrete))
                    return false;
            }

            return true;
        }

        public ConcreteType CommonAncestor 
            => AncestorTypes.Any() ? AncestorTypes.GetCommonDescendantOrNull() : null;

        public ConcreteType CommonDescedant 
            => DescedantTypes.Any() ? DescedantTypes.GetCommonAncestor() : null;

        public List<ConcreteType> AncestorTypes { get; } = new List<ConcreteType>();
        public List<ConcreteType> DescedantTypes { get; } = new List<ConcreteType>();
        public ConcreteType PreferedType { get; set; }

        public void Validate()
        {
            if(!AncestorTypes.Any())
                return;
            if(!DescedantTypes.Any())
                return;

            var des = CommonDescedant?? throw new InvalidOperationException();
            
            if(!des.CanBeImplicitlyConvertedTo(CommonAncestor))
                throw new InvalidOperationException();
        }

        public override string ToString() => $"[{CommonDescedant}..{CommonAncestor}]";
    }
}
