namespace nfun.Ti4
{
    public interface IType: ISolvingState
    {
        bool IsSolved { get; }
        IType GetLastCommonAncestorOrNull(IType otherType);
        bool CanBeImplicitlyConvertedTo(PrimitiveType type);
    }
}