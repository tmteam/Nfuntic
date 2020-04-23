using System;

namespace nfun.Ti4
{
    public class Fun : IType, IState
    {
        public static Fun Of(IState returnType, IState argType)
        {
            SolvingNode argNode = null;
            SolvingNode retNode = null;

            if (returnType is IType rt)
                retNode = SolvingNode.CreateTypeNode(rt);
            else if(returnType is RefTo retRef)
                retNode = retRef.Node;
            else
                throw new InvalidOperationException();

            if (argType is IType at)
                argNode = SolvingNode.CreateTypeNode(at);
            else if (argType is RefTo aRef)
                argNode = aRef.Node;
            else
                throw new InvalidOperationException();

            return new Fun(argNode, retNode);
        }
        public static Fun Of(IType returnType, IType argType)
        {
            return new Fun(
                argNode:    SolvingNode.CreateTypeNode(argType),    
                retNode: SolvingNode.CreateTypeNode(returnType));
        }
        public static Fun Of(SolvingNode returnNode, SolvingNode argNode) 
            => new Fun(argNode, returnNode);

        private Fun(SolvingNode argNode, SolvingNode retNode)
        {
            ArgNode = argNode;
            RetNode = retNode;
        }

        public object ReturnType => RetNode.State;
        public object ArgType => ArgNode.State;

        public SolvingNode RetNode { get; }
        public SolvingNode ArgNode { get; }

        public bool IsSolved => RetNode.IsSolved && ArgNode.IsSolved;
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

        public override string ToString() => $"({ArgType}->{ReturnType})";
    }
}