namespace nfun.Ti4
{
    public class ArrayOf: IType
    {
        public ArrayOf(SolvingNode elementNode)
        {
            ElementNode = elementNode;
        }

        public static ArrayOf Create(IType type)
        {
            var node = new SolvingNode(
                name: type.ToString(), 
                state: type, 
                type: SolvingNodeType.TypeVariable);
            return new ArrayOf(node);
        }

        public SolvingNode ElementNode { get; }
        public bool IsSolved => ElementNode.IsSolved;
        public object Element => ElementNode.State;

        public bool TrySetAncestor(PrimitiveType ancestorType) 
            => ancestorType.Equals(PrimitiveType.Any);

        public override string ToString() => $"ArrayOf({ElementNode})";
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
    }
}