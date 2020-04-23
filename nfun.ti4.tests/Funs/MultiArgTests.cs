using System.Collections.Generic;
using System.Text;
using nfun.Ti4;
using NUnit.Framework;

namespace nfun.ti4.tests.Funs
{
    public class MultiArgTests
    {
        [Test]
        public void GenericReduce_GetSum()
        {
            //        5  0  4      132
            //y = reduce(x, f(a,b)=a+b)
            var graph = new GraphBuilder();
            
            graph.SetVar("x", 0);
            graph.SetVar("la", 1);
            graph.SetVar("lb", 2);
            graph.SetArith(1,2,3);
            graph.CreateLambda(3, 4, "la","lb");
            SetReduceCall(graph, 0, 4, 5);
            graph.SetDef("y", 5);

            var result = graph.Solve();

            var t = result.AssertAndGetSingleArithGeneric();

            result.AssertAreGenerics(t, "y","la","lb");
            result.AssertNamed(Array.Of(t), "x");
            result.AssertNode(Fun.Of(new []{t,t}, t), 4);
        }

        

        [Test]
        public void ReduceConcreteOut_GetSum()
        {
            //            5  0  4      132
            //y:u32 = reduce(x, f(a,b)=a+b)
            var graph = new GraphBuilder();

            graph.SetVar("x", 0);
            graph.SetVar("la", 1);
            graph.SetVar("lb", 2);
            graph.SetArith(1, 2, 3);
            graph.CreateLambda(3, 4, "la", "lb");
            SetReduceCall(graph, 0, 4, 5);
            graph.SetVarType("y", Primitive.U32);
            graph.SetDef("y", 5);

            var result = graph.Solve();

            result.AssertNoGenerics();

            result.AssertNamed(Primitive.U32, "y", "la", "lb");
            result.AssertNamed(Array.Of(Primitive.U32), "x");
            result.AssertNode(Fun.Of(new[] { Primitive.U32, Primitive.U32 }, Primitive.U32), 4);
        }

        [Test]
        public void  ReduceConcreteArg_GetSum()
        {
            //                 5  0  4      132
            //x:u32[]; y = reduce(x, f(a,b)=a+b)
            var graph = new GraphBuilder();

            graph.SetVarType("y", Primitive.U32);
            graph.SetVar("x", 0);
            graph.SetVar("la", 1);
            graph.SetVar("lb", 2);
            graph.SetArith(1, 2, 3);
            graph.CreateLambda(3, 4, "la", "lb");
            SetReduceCall(graph, 0, 4, 5);
            graph.SetDef("y", 5);

            var result = graph.Solve();

            result.AssertNoGenerics();

            result.AssertNamed(Primitive.U32, "y", "la", "lb");
            result.AssertNamed(Array.Of(Primitive.U32), "x");
            result.AssertNode(Fun.Of(new[] { Primitive.U32, Primitive.U32 }, Primitive.U32), 4);
        }


        [Test]
        public void GenericFold_AllIsNan()
        {
            //      6  0  5      1  4    3   2
            //y = fold(x, f(a,b)=a and isNan(b))
            var graph = new GraphBuilder();

            graph.SetVar("x", 0);
            graph.SetVar("la", 1);
            graph.SetVar("lb", 2);
            graph.SetCall(new []{Primitive.Real, Primitive.Bool},new []{2,3});
            graph.SetBoolCall(1,3,4);
            graph.CreateLambda(4, 5, "la", "lb");
            SetFoldCall(graph, 0, 5, 6);
            var result = graph.Solve();

            result.AssertNoGenerics();
    
            result.AssertNamed(Array.Of(Primitive.Real), "x");
            result.AssertNamed(Primitive.Real, "lb");
            result.AssertNamed(Primitive.Bool, "la","y");
        }

        private static void SetReduceCall(GraphBuilder graph, int arrId, int funId, int returnId )
        {
            var generic = graph.InitializeVarNode();

            graph.SetCall(new IState[]
            {
                Array.Of(generic),
                Fun.Of(new[] {generic, generic}, generic),
                generic
            }, new []{arrId, funId, returnId});
        }

        private static void SetFoldCall(GraphBuilder graph, int arrId, int funId, int returnId)
        {
            var inT = graph.InitializeVarNode();
            var outT = graph.InitializeVarNode();

            graph.SetCall(new IState[]
            {
                Array.Of(inT),
                Fun.Of(new[] {inT, outT}, outT),
                outT
            }, new[] { arrId, funId, returnId });
        }
    }
}
