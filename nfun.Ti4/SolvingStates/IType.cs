namespace NFun.Tic.SolvingStates
{
    public interface IType: IState
    {
        bool IsSolved { get; }
        IType GetLastCommonAncestorOrNull(IType otherType);
        bool CanBeImplicitlyConvertedTo(Primitive type);
    }
}