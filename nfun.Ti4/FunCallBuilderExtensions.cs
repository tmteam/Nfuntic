using System.Linq;

namespace nfun.Ti4
{
    public static class FunCallBuilderExtensions
    {
        public static void SetCall(this GraphBuilder builder, PrimitiveType typesOfTheCall, params int[] argumentsThenResult)
        {
            var types = argumentsThenResult.Select(s => (ISolvingState)typesOfTheCall).ToArray();
            builder.SetCall(types, argumentsThenResult);
        }

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
        
        public static void SetBitwiseInvert(this GraphBuilder builder, int argId, int resultId)
        {
            var t = builder.InitializeVarNode(PrimitiveType.U8, PrimitiveType.I96);

            builder.SetCall(
                argThenReturnTypes: new ISolvingState[] { t, t },
                argThenReturnIds: new[] { argId, resultId});
        }

        public static void SetBitwise(this GraphBuilder builder, int leftId, int rightId, int resultId)
        {
            var t = builder.InitializeVarNode(PrimitiveType.U8, PrimitiveType.I96);

            builder.SetCall(
                argThenReturnTypes: new ISolvingState[] { t, t,t },
                argThenReturnIds: new[] { leftId,rightId, resultId });
        }

        public static void SetBitShift(this GraphBuilder builder, int leftId, int rightId, int resultId)
        {
            var t = builder.InitializeVarNode(PrimitiveType.U24, PrimitiveType.I96);

            builder.SetCall(
                argThenReturnTypes: new ISolvingState[] { t, PrimitiveType.I48, t },
                argThenReturnIds: new[] { leftId, rightId, resultId });
        }

        public static void SetArith(this GraphBuilder builder, int leftId, int rightId, int resultId)
        {
            var t = builder.InitializeVarNode(PrimitiveType.U24, PrimitiveType.Real);

            builder.SetCall(
                argThenReturnTypes: new ISolvingState[] { t, t, t },
                argThenReturnIds: new[] { leftId, rightId, resultId });
        }

        public static void SetNegateCall(this GraphBuilder builder,int argId, int resultId)
        {
            var t = builder.InitializeVarNode(PrimitiveType.I16, PrimitiveType.Real);

            builder.SetCall(
                argThenReturnTypes: new ISolvingState[] { t, t },
                argThenReturnIds: new[] { argId, resultId });
        }

        public static void SetArrGetCall(this GraphBuilder builder, int arrArgId, int indexArgId, int resId)
        {
            var varNode = builder.InitializeVarNode();
            builder.SetCall(
                new ISolvingState []{ArrayOf.Create(varNode), PrimitiveType.I32, varNode },new []{arrArgId,indexArgId, resId});
        }
        
        public static void SetConcatCall(this GraphBuilder builder, int firstId, int secondId, int resultId)
        {
            var varNode = builder.InitializeVarNode();
            
            builder.SetCall(new ISolvingState[]
            {
                ArrayOf.Create(varNode),ArrayOf.Create(varNode),ArrayOf.Create(varNode),
            }, new []{firstId, secondId, resultId});

        }

        public static void SetSumCall(this GraphBuilder builder, int argId, int resultId)
        {
            var varNode = builder.InitializeVarNode(PrimitiveType.U24, PrimitiveType.Real);

            builder.SetCall(new ISolvingState[]{ArrayOf.Create(varNode), varNode}, new []{argId,resultId});
        }
    }
}
