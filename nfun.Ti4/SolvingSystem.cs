using System;
using System.Collections.Generic;

namespace nfun.Ti4
{

    public class ConcreteTypeSolvingNode : SolvingNode
    {
        public ConcreteTypeSolvingNode(PrimitiveTypeName typeName):base(typeName.ToString())
        {
            this.NodeState = new ConcreteType(typeName);
        }
    }
    public class SolvingNode
    {
        public SolvingNode(string name)
        {
            Name = name;
        }
        public bool IsSolved => NodeState is ConcreteType;
        public object NodeState { get; set; }

        public string Name { get; }

        public static ConcreteTypeSolvingNode Any { get; } = new ConcreteTypeSolvingNode(PrimitiveTypeName.Any);
        public static ConcreteTypeSolvingNode Bool { get; } = new ConcreteTypeSolvingNode(PrimitiveTypeName.Bool);
        public static ConcreteTypeSolvingNode Char { get; } = new ConcreteTypeSolvingNode(PrimitiveTypeName.Char);
        public static ConcreteTypeSolvingNode Real { get; } = new ConcreteTypeSolvingNode(PrimitiveTypeName.Real);
        public static ConcreteTypeSolvingNode I96 { get; } = new ConcreteTypeSolvingNode(PrimitiveTypeName.I96);
        public static ConcreteTypeSolvingNode I64 { get; } = new ConcreteTypeSolvingNode(PrimitiveTypeName.I64);
        public static ConcreteTypeSolvingNode I48 { get; } = new ConcreteTypeSolvingNode(PrimitiveTypeName.I48);
        public static ConcreteTypeSolvingNode I32 { get; } = new ConcreteTypeSolvingNode(PrimitiveTypeName.I32);
        public static ConcreteTypeSolvingNode I24 { get; } = new ConcreteTypeSolvingNode(PrimitiveTypeName.I24);
        public static ConcreteTypeSolvingNode I16 { get; } = new ConcreteTypeSolvingNode(PrimitiveTypeName.I16);
        public static ConcreteTypeSolvingNode U64 { get; } = new ConcreteTypeSolvingNode(PrimitiveTypeName.U64);
        public static ConcreteTypeSolvingNode U48 { get; } = new ConcreteTypeSolvingNode(PrimitiveTypeName.U48);
        public static ConcreteTypeSolvingNode U32 { get; } = new ConcreteTypeSolvingNode(PrimitiveTypeName.U32);
        public static ConcreteTypeSolvingNode U24 { get; } = new ConcreteTypeSolvingNode(PrimitiveTypeName.U24);
        public static ConcreteTypeSolvingNode U16 { get; } = new ConcreteTypeSolvingNode(PrimitiveTypeName.U16);
        public static ConcreteTypeSolvingNode U12 { get; } = new ConcreteTypeSolvingNode(PrimitiveTypeName.U12);
        public static ConcreteTypeSolvingNode U8 { get; } = new ConcreteTypeSolvingNode(PrimitiveTypeName.U8);
    }

    public class ReferenceSolvingState
    {
        public SolvingNode RefTo { get; set; }
    }

    public class ConstrainsSolvingState
    {
        public List<SolvingNode> Ancestors { get; } = new List<SolvingNode>();
        public List<SolvingNode> Descedants { get;  } = new List<SolvingNode>();
        public ConcreteType PreferedType { get; set; }
    }




}
