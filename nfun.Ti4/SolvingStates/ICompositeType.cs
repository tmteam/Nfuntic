using System.Collections.Generic;

namespace nfun.Ti4
{
    interface ICompositeType : IType
    {
        IEnumerable<SolvingNode> Members { get; }
    }
}