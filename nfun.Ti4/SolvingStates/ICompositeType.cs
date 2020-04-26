using System.Collections.Generic;

namespace nfun.Ti4
{
    public interface ICompositeType : IType
    {
        ICompositeType GetNonReferenced();
        IEnumerable<SolvingNode> Members { get; }
    }
}