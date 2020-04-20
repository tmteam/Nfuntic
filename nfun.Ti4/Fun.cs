namespace nfun.Ti4
{
    public class Fun : IType
    {
        public static Fun Of(IType returnType, IType argType)
        {
            return new Fun(
                argNode:    SolvingNode.CreateTypeNode(argType),    
                returnNode: SolvingNode.CreateTypeNode(returnType));
        }
        public static Fun Of(SolvingNode returnNode, SolvingNode argNode) 
            => new Fun(argNode, returnNode);

        private Fun(SolvingNode argNode, SolvingNode returnNode)
        {
            ArgNode = argNode;
            ReturnNode = returnNode;
        }

        public object ReturnType => ReturnNode.State;
        public object ArgType => ArgNode.State;

        public SolvingNode ReturnNode { get; }
        public SolvingNode ArgNode { get; }

        public bool IsSolved => ReturnNode.IsSolved && ArgNode.IsSolved;
        public IType GetLastCommonAncestorOrNull(IType otherType)
        {
            var funType = otherType as Fun;
            
            if (funType == null)
                return PrimitiveType.Any;
            
            if(!(ReturnType is IType returnType) || !(ArgType is IType argType))
                return null;
            if (!(funType.ReturnType is IType returnTypeB) || !(funType.ArgType is IType argTypeB))
                return null;
            if (!returnType.IsSolved || !returnTypeB.IsSolved)
                return null;
            if (!argType.IsSolved || !argTypeB.IsSolved)
                return null;

            var returnAnc = returnType.GetLastCommonAncestorOrNull(returnTypeB);

            if(argType.Equals(argTypeB))
                return Fun.Of(
                    returnType: returnAnc, 
                    argType:    argType);

            if (argType is PrimitiveType primitiveA && argTypeB is PrimitiveType primitiveB)
            {
                var argDesc = primitiveA.GetFirstCommonDescendantOrNull(primitiveB);
                if (argDesc != null) 
                    return Of(returnAnc, argDesc);
            }

            //todo не рассмотрен случай для поиска общих наследников для непримитивных типов
            return null;
        }

        public bool CanBeImplicitlyConvertedTo(PrimitiveType type) 
            => type.Equals(PrimitiveType.Any);

        public override bool Equals(object obj)
        {
            if (!(obj is Fun fun))
                return false;
            return fun.ArgType.Equals(ArgType) && fun.ReturnType.Equals(ReturnType);
        }

        public override string ToString()
        {
            return $"({ArgType}->{ReturnType})";
        }
    }
}