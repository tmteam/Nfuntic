﻿using System;
using NFun.Tic.SolvingStates;
using Array = NFun.Tic.SolvingStates.Array;

namespace NFun.Tic.PlaygroundApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ReqDef();
            Console.ReadLine();
        }

        static void ReqDef3()
        {
            //    1 0              
            //x = [ x]
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetArrayInit(1, 0);
            graph.SetDef("x", 1);
            graph.Solve();
        }
        static void ReqDef4()
        {
            //   2 1 0              
            //x =[ [ x] ]
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetArrayInit(1, 0);
            graph.SetArrayInit(2, 1);
            graph.SetDef("x", 2);
            graph.Solve();
        }

        static void ReqDef2()
        {
            //    2 0 1              
            //x = [ x,x]
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetVar("x", 1);
            graph.SetArrayInit(2, 0, 1);
            try
            {
                graph.SetDef("x", 2);
                graph.Solve();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        static void ReqDef()
        {
            //    4 1 0  3 2           
            //x = [ [ a],[ x]]
            var graph = new GraphBuilder();
            graph.SetVar("a", 0);
            graph.SetArrayInit(1, 0);
            graph.SetVar("x", 2);
            graph.SetArrayInit(3, 2);
            graph.SetArrayInit(4, 1, 3);
            graph.SetDef("x", 4);
            graph.Solve();
        }
        static void LambdaEx()
        {
            //        5  0  4              132
            //y = reduce(x, f(a,b:i32):i64=a+b)
            var graph = new GraphBuilder();

            graph.SetVar("x", 0);
            graph.SetVar("la", 1);
            graph.SetVarType("lb", Primitive.I32);
            graph.SetVar("lb", 2);
            graph.SetArith(1, 2, 3);
            graph.CreateLambda(3, 4, Primitive.I64, "la", "lb");
            graph.SetReduceCall(0, 4, 5);
            graph.SetDef("y", 5);

            var result = graph.Solve();

        }
        static void SumGenericEx()
        {
            //     3  2 0,  1  
            //y = sum([ 1, -1]) 
            var graph = new GraphBuilder();
            graph.SetIntConst(0, Primitive.U8);
            graph.SetIntConst(1, Primitive.I16);
            graph.SetArrayInit(2, 0, 1);
            graph.SetSumCall(2, 3);
            graph.SetDef("y", 3);
            var result = graph.Solve();

        }
        static void ConcatEx()
        {
            //              2     0 1
            //a:int[]; y = concat(a,b) 
            var graph = new GraphBuilder();
            graph.SetVarType("a", Array.Of(Primitive.I32));
            graph.SetVar("a", 0);
            graph.SetVar("b", 1);
            graph.SetConcatCall(0, 1, 2);
            graph.SetDef("y", 2);
            graph.Solve();

        }

        static void ConcreteConcatEx()
        {
                //              2     0 1
                //a:int[]; y = concat(a,b) 
                var graph = new GraphBuilder();
                graph.SetVarType("a", Array.Of(Primitive.I32));
                graph.SetVar("a", 0);
                graph.SetVar("b", 1);
                graph.SetConcatCall(0, 1, 2);
                graph.SetDef("y", 2);
                var result = graph.Solve();
       }
    }
}
