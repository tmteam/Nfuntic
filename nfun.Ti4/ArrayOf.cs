namespace nfun.Ti4
{
    public class ArrayOf
    {
        public ArrayOf(SolvingNode elementNode)
        {
            ElementNode = elementNode;
        }

        public SolvingNode ElementNode { get; }

        public bool TrySetAncestor(ConcreteType ancestorType)
        {
            if (!(ancestorType is ConcreteArrayType array))
                return false;
            return ElementNode.TrySetAncestor(array.ElementType);
        }

        public override string ToString() => $"ArrayOf({ElementNode})";
    }
}