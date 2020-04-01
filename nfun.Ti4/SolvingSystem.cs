using System;
using System.Collections.Generic;

namespace nfun.Ti4
{

    public class ConcreteTypeSolvingNode : SolvingNode
    {
        public ConcreteTypeSolvingNode(PrimitiveTypeName typeName, string nodeName):base(nodeName)
        {
            this.NodeState = new ConcreteType(typeName);
        }
    }
    public class SolvingNode
    {
        public int GraphId { get; set; }
        public SolvingNode(string name)
        {
            Name = name;
        }
        public List<SolvingNode> Ancestors { get; } = new List<SolvingNode>();

        public bool IsSolved => NodeState is ConcreteType;
        public object NodeState { get; set; }

        public string Name { get; }
        public override string ToString()
        {
            return Name + "." + NodeState?.GetType().Name;
        }
    }

    public class ReferenceSolvingState
    {
        public SolvingNode RefTo { get; set; }
    }

    public class ConstrainsSolvingState
    {
        public List<ConcreteType> AncestorTypes { get; } = new List<ConcreteType>();
        public List<ConcreteType> DescedantTypes { get;  } = new List<ConcreteType>();
        public ConcreteType PreferedType { get; set; }
    }




}
