namespace nfun.Ti4
{
    public interface IType: IState
    {
        bool IsSolved { get; }
        IType GetLastCommonAncestorOrNull(IType otherType);
        bool CanBeImplicitlyConvertedTo(Primitive type);
    }
}