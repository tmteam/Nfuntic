using System;
using System.Collections.Generic;
using System.Text;
using nfun.Ti4;
using NUnit.Framework;
using Array = nfun.Ti4.Array;

namespace nfun.ti4.tests.Funs
{
    public class GetFirstTests
    {
        [Test]
        public void StrictArrayArg()
        {
            //     6  1 0    5  2  4 3
            //y = First([ 1i ], x->x==0)
            var graph = new GraphBuilder();
            graph.SetConst(0, Primitive.I32);
            graph.SetArrayInit(1, 0);
            graph.SetVar("lx", 2);
            graph.SetIntConst(3, Primitive.U8);
            graph.SetEquality(2, 3, 4);
            graph.CreateLambda(4, 5, "lx");
            graph.SetGetFirst(1, 5, 6);
            graph.SetDef("y", 6);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(Primitive.I32, "y");
            result.AssertNamed(Primitive.I32, "lx");
            result.AssertNode(Fun.Of(argType: Primitive.I32, retType: Primitive.Bool), 5);
        }
        [Test]
        public void StrictArrayAndLambdaArg()
        {
            //       6  1 0         5  2 4 3
            //y = first([ 1i ], x:int->x== 0)
            var graph = new GraphBuilder();
            graph.SetConst(0, Primitive.I32);
            graph.SetArrayInit(1, 0);
            graph.SetVarType("lx", Primitive.I32);
            graph.SetVar("lx", 2);
            graph.SetIntConst(3, Primitive.U8);
            graph.SetEquality(2, 3, 4);
            graph.CreateLambda(4, 5, "lx");
            graph.SetGetFirst(1, 5, 6);
            graph.SetDef("y", 6);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(Primitive.I32, "y");
            result.AssertNamed(Primitive.I32, "lx");
            result.AssertNode(Ti4.Fun.Of(retType: Primitive.Bool, argType: Primitive.I32), 5);
        }

        [Test]
        public void LambdaArgDowncast_Throws()
        {
            //       6  1 0          5  2 43
            //y = First([ 1.0 ], x:int->x==0)
            var graph = new GraphBuilder();
            graph.SetConst(0, Primitive.Real);
            graph.SetArrayInit(1, 0);
            graph.SetVarType("lx", Primitive.I32);
            graph.SetVar("lx", 2);
            graph.SetIntConst(3, Primitive.U8);
            graph.SetEquality(2, 3, 4);
            try
            {
                graph.CreateLambda(4, 5, "lx");
                graph.SetGetFirst(1, 5, 6);
                graph.SetDef("y", 6);
                graph.Solve();
                Assert.Fail("Impossible equation solved");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Test]
        [Ignore("Upcast for complex types")]
        public void ArgUpcastStrictArrayArg()
        {
            //       6  1 0     5       2 4 3
            //y = First([ 1i ], x:real->x ==0)
            var graph = new GraphBuilder();
            graph.SetConst(0, Primitive.I32);
            graph.SetArrayInit(1, 0);
            graph.SetVarType("lx", Primitive.Real);
            graph.SetVar("lx", 2);
            graph.SetIntConst(3, Primitive.U8);
            graph.SetEquality(2, 3, 4);
            graph.CreateLambda(4, 5, "lx");
            graph.SetGetFirst(1, 5, 6);
            graph.SetDef("y", 6);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(Primitive.Real, "x");
            result.AssertNode(Ti4.Fun.Of(retType: Primitive.Bool, argType: Primitive.Real), 5);
        }
        [Test]
        public void BoolArrayArg()
        {
            //       3  0  2  1
            //y = First(a, x->x)
            var graph = new GraphBuilder();
            graph.SetVar("a", 0);
            graph.SetVar("2lx", 1);
            graph.CreateLambda(1, 2, "2lx");
            graph.SetGetFirst(0, 2, 3);
            graph.SetDef("y", 3);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(Array.Of(Primitive.Bool), "a");
            result.AssertNamed(Primitive.Bool, "2lx");
            result.AssertNamed(Primitive.Bool, "y");
            result.AssertNode(Ti4.Fun.Of(
                argType: Primitive.Bool,
                retType: Primitive.Bool), 2);
        }

        [Test]
        public void ConcreteFun()
        {
            //       2  0   1
            //y = First(a, isNan)
            var graph = new GraphBuilder();
            graph.SetVar("a", 0);
            graph.SetVarType("isNan", Fun.Of(
                 argType: Primitive.Real,
                 retType: Primitive.Bool));
            graph.SetVar("isNan", 1);
            graph.SetGetFirst(0, 1, 2);
            graph.SetDef("y", 2);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(Array.Of(Primitive.Real), "a");
        }

        [Test]
        public void ConcreteFunAndUpcast()
        {
            //                2  0   1
            //a:int[]; y = First(a, isNan)
            var graph = new GraphBuilder();
            graph.SetVarType("a", Array.Of(Primitive.I32));
            graph.SetVar("a", 0);
            graph.SetVarType("isNan", Fun.Of(
                argType: Primitive.Real,
                retType: Primitive.Bool));
            graph.SetVar("isNan", 1);
            graph.SetGetFirst(0, 1, 2);
            graph.SetDef("y", 2);
            var result = graph.Solve();
            result.AssertNoGenerics();
        }
    }
}
