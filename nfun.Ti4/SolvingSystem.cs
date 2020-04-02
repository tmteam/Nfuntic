using System;
using System.Collections.Generic;
using System.Linq;

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
        public override string ToString() => Name + "." + NodeState?.GetType().Name;

        public void PrintToConsole()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{Name}:");
            Console.ResetColor();

            if (NodeState is ConcreteType concrete)
                Console.WriteLine($"{concrete.Name} ");
            else if (NodeState is ReferenceSolvingState reference)
                Console.WriteLine($"{reference.RefTo.Name} ");
            else if (NodeState is ConstrainsSolvingState constrains)
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

    public static class SolvingStateMergeFunctions
    {
        public static object Merge(ConcreteType a, ConcreteType b)
        {
            if (a.Name != b.Name)
                throw new InvalidOperationException();
            return a;
        }

        public static object Merge(ConstrainsSolvingState limits, ConcreteType concrete)
        {
            if (!limits.AncestorTypes.TrueForAll(concrete.CanBeImplicitlyConvertedTo))
                throw new InvalidOperationException();
            if (!limits.DescedantTypes.TrueForAll(d => d.CanBeImplicitlyConvertedTo(concrete)))
                throw new InvalidOperationException();
            return concrete;
        }

        public static object Merge(ConstrainsSolvingState a, ConstrainsSolvingState b)
        {
            var result = new ConstrainsSolvingState();
            ConcreteType ancestor = null;

            if (a.AncestorTypes.Any() || b.AncestorTypes.Any())
            {
                ancestor = ConcreteType.GetLastCommonAncestor(a.AncestorTypes.Union(b.AncestorTypes));
                result.AncestorTypes.Add(ancestor);
            }

            ConcreteType descendant = null;

            if (a.DescedantTypes.Any() || b.DescedantTypes.Any())
            {
                descendant = ConcreteType.GetFirstCommonDescendantOrNull(a.DescedantTypes.Union(b.DescedantTypes));
                if (descendant == null)
                    throw new InvalidOperationException();
                result.DescedantTypes.Add(descendant);
            }

            if (ancestor != null && descendant != null)
                if (!descendant.CanBeImplicitlyConvertedTo(ancestor))
                    throw new InvalidOperationException();
            if (a.PreferedType != null)
            {
                if (b.PreferedType == null || a.PreferedType.Name == b.PreferedType.Name)
                {
                    result.PreferedType = a.PreferedType;
                }
            }
            else
                result.PreferedType = b.PreferedType;

            return result;
        }
    }

    public class ReferenceSolvingState
    {
        public SolvingNode RefTo { get; set; }
    }

    public class ConstrainsSolvingState
    {
        public List<ConcreteType> AncestorTypes { get; } = new List<ConcreteType>();
        public List<ConcreteType> DescedantTypes { get; } = new List<ConcreteType>();
        public ConcreteType PreferedType { get; set; }

        public void Validate()
        {
            if(!AncestorTypes.Any())
                return;
            if(!DescedantTypes.Any())
                return;
            var anc =  ConcreteType.GetLastCommonAncestor(AncestorTypes);
            var des = ConcreteType.GetFirstCommonDescendantOrNull(DescedantTypes);
            if(des==null)
                throw new InvalidOperationException();
            if(!des.CanBeImplicitlyConvertedTo(anc))
                throw new InvalidOperationException();
        }
    }




}
