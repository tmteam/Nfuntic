﻿using System;
using nfun.Ti4;
using NUnit.Framework;
using Array = nfun.Ti4.Array;

namespace nfun.ti4.tests.Arrays
{
    public class ConcreteArrayFunTest
    {
        [Test(Description = "y = x.NoNans()")]
        public void ConcreteCall()
        {
            //        1  0
            //y = NoNans(x) 
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetCall(new  IState[]{Array.Of(Primitive.Real), Primitive.Bool}, new []{0,1});
            graph.SetDef("y", 1);
            
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(Array.Of(Primitive.Real), "x");
            result.AssertNamed(Primitive.Bool, "y");
        }

        [Test(Description = "x:int[]; y = x.NoNans()")]
        public void ConcreteCall_WithUpCast()
        {
            //                 1  0
            //x:int[]; y = NoNans(x) 
            var graph = new GraphBuilder();
            graph.SetVarType("x", Array.Of(Primitive.I32));
            graph.SetVar("x", 0);
            graph.SetCall(new IState[] { Array.Of(Primitive.Real), Primitive.Bool }, new[] { 0, 1 });
            graph.SetDef("y", 1);

            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(Array.Of(Primitive.I32), "x");
            result.AssertNamed(Primitive.Bool, "y");
        }


        [Test(Description = "y = [1i,-1i].NoNans()")]
        //[Ignore("Upcast for complex types")]
        public void ConcreteCall_WithGenericArray()
        {
            //        3   2 0  1
            //y = NoNans( [ 1, -1]) 
            var graph = new GraphBuilder();
            graph.SetIntConst(0, Primitive.U8);
            graph.SetIntConst(1, Primitive.I16);
            graph.SetArrayInit(2, 0, 1);

            graph.SetCall(new IState[] { Array.Of(Primitive.Real), Primitive.Bool }, new[] { 2, 3 });
            graph.SetDef("y", 3);

            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNode(Array.Of(Primitive.Real),2);
            result.AssertNode(Primitive.Real, 0,1);
            result.AssertNamed(Primitive.Bool, "y");
        }

        [Test]
        public void ImpossibleArgType_Throws()
        {
            //                 1  0
            //x:Any[]; y = NoNans(x)
            var graph = new GraphBuilder();
            graph.SetVarType("x", Array.Of(Primitive.Any));
            graph.SetVar("x", 0);
            
            try
            {
                
                graph.SetCall(new IState[] { Array.Of(Primitive.Real), Primitive.Bool }, new[] { 0, 1 });
                graph.SetDef("y", 1);
                graph.Solve();
                Assert.Fail();
            }
            catch (Exception e) 
            {
                Console.WriteLine(e);
            }
            
        }
    }
}