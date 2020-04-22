namespace nfun.Ti4
{
    public class ArrayOf: IType, ISolvingState
    {
        public ArrayOf(SolvingNode elementNode)
        {
            ElementNode = elementNode;
        }

        public static ArrayOf Create(SolvingNode node) 
            => new ArrayOf(node);

        public static ArrayOf Create(IType type) 
            => new ArrayOf(SolvingNode.CreateTypeNode(type));

        public SolvingNode ElementNode { get; }
        public bool IsSolved => (Element as IType)?.IsSolved == true;
        public object Element => ElementNode.State;


        public override string ToString()
        {
            if(ElementNode.IsSolved)
                return $"arr({ElementNode})";

            return $"arr({ElementNode.Name})";
        }

        public IType GetLastCommonAncestorOrNull(IType otherType)
        {
            var arrayType = otherType as ArrayOf;
            if (arrayType == null)
                return PrimitiveType.Any;
            var elementTypeA = Element as IType;
            if (elementTypeA == null)
                return null;
            var elementTypeB = arrayType.Element as IType;
            if (elementTypeB == null)
                return null;
            var ancestor = elementTypeA.GetLastCommonAncestorOrNull(elementTypeB);
            if (ancestor == null)
                return null;
            return ArrayOf.Create(ancestor);
        }

        public bool CanBeImplicitlyConvertedTo(PrimitiveType type) 
            => type.Equals(PrimitiveType.Any);

        public override bool Equals(object obj)
        {
            if (obj is ArrayOf arr)
            {
                return arr.Element.Equals(this.Element);
            }
            return false;
        }
    }
}