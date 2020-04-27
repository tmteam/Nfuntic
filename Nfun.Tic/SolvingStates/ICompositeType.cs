using System.Collections.Generic;

namespace NFun.Tic.SolvingStates
{
    public interface ICompositeType : IType
    {
        ICompositeType GetNonReferenced();
        IEnumerable<SolvingNode> Members { get; }
        IEnumerable<SolvingNode> AllLeafTypes { get; }
    }
}