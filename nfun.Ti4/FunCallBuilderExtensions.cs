using System;
using System.Collections.Generic;
using System.Text;

namespace nfun.Ti4
{
    public static class FunCallBuilderExtensions
    {
        public static void SetEquality(this GraphBuilder builder, int leftId, int rightId, int resultId)
        {
            var t = builder.InitializeVarNode();
            
            builder.SetCall(
                argThenReturnTypes: new ISolvingState []{t, t, PrimitiveType.Bool},
                argThenReturnIds: new []{leftId, rightId, resultId});
        }
        
        public static void SetComparable(this GraphBuilder builder,int leftId, int rightId, int resultId)
        {
            var t = builder.InitializeVarNode(isComparable: true);

            builder.SetCall(
                argThenReturnTypes: new ISolvingState[] { t, t, PrimitiveType.Bool },
                argThenReturnIds: new[] { leftId, rightId, resultId });
        }
        /*
        public void SetBitwiseInvert(int argId, int resultId)
        {
            var arg = GetOrCreateNode(argId);
            var result = GetOrCreateNode(resultId);

            var varNode = CreateVarType(new SolvingConstrains(PrimitiveType.U8, PrimitiveType.I96));

            varNode.BecomeReferenceFor(result);
            varNode.BecomeAncestorFor(arg);
        }

        public void SetBitwise(int leftId, int rightId, int resultId)
        {
            var left = GetOrCreateNode(leftId);
            var right = GetOrCreateNode(rightId);
            var result = GetOrCreateNode(resultId);

            var varNode = CreateVarType(new SolvingConstrains(PrimitiveType.U8, PrimitiveType.I96));

            varNode.BecomeReferenceFor(result);
            varNode.BecomeAncestorFor(left);
            varNode.BecomeAncestorFor(right);
        }

        public void SetBitShift(int leftId, int rightId, int resultId)
        {
            var left = GetOrCreateNode(leftId);
            SetOrCreateConcrete(rightId, PrimitiveType.I48);
            var result = GetOrCreateNode(resultId);

            var varNode = CreateVarType(new SolvingConstrains(PrimitiveType.U24, PrimitiveType.I96));

            varNode.BecomeReferenceFor(result);
            varNode.BecomeAncestorFor(left);
        }

        public void SetArith(int leftId, int rightId, int resultId)
        {
            var left = GetOrCreateNode(leftId);
            var right = GetOrCreateNode(rightId);
            var result = GetOrCreateNode(resultId);

            var varNode = CreateVarType(new SolvingConstrains(PrimitiveType.U24, PrimitiveType.Real));

            varNode.BecomeReferenceFor(result);

            varNode.BecomeAncestorFor(left);
            varNode.BecomeAncestorFor(right);
        }

        public void SetNegateCall(int argId, int resultId)
        {
            var vartype = CreateVarType(new SolvingConstrains(PrimitiveType.I16, PrimitiveType.Real));
            var arg = GetOrCreateNode(argId);
            var res = GetOrCreateNode(resultId);
            vartype.BecomeReferenceFor(res);
            vartype.BecomeAncestorFor(arg);
        }

        public void SetArrGetCall(int arrArgId, int indexArgId, int resId)
        {
            var vartype = CreateVarType();
            GetOrCreateArrayNode(arrArgId, vartype);
            GetOrCreateNode(indexArgId).SetAncestor(PrimitiveType.I32);
            var result = GetOrCreateNode(resId);
            vartype.BecomeReferenceFor(result);
        }

        public void SetConcatCall(int firstId, int secondId, int resultId)
        {
            var varElementType = CreateVarType();
            var arrType = CreateVarType(new ArrayOf(varElementType));
            var first = GetOrCreateNode(firstId);
            arrType.BecomeAncestorFor(first);
            var second = GetOrCreateNode(secondId);
            arrType.BecomeAncestorFor(second);

            var result = GetOrCreateNode(resultId);
            result.State = new RefTo(arrType);
        }

        public void SetSumCall(int argId, int resultId)
        {
            var vartype = CreateVarType(new SolvingConstrains(PrimitiveType.U24, PrimitiveType.Real));
            GetOrCreateArrayNode(argId, vartype);
            var result = GetOrCreateNode(resultId);
            vartype.BecomeReferenceFor(result);
        }*/
    }
}
