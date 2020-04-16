using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nfun.Ti4;

namespace nfun.ti4.app
{
    class Program
    {
        static void Main(string[] args)
        {
            SumGenericEx();
            Console.ReadLine();
        }

        static void SumGenericEx()
        {
            //     3  2 0,  1  
            //y = sum([ 1, -1]) 
            var graph = new GraphBuilder();
            graph.SetIntConst(0, PrimitiveType.U8);
            graph.SetIntConst(1, PrimitiveType.I16);
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
            graph.SetVarType("a", ArrayOf.Create(PrimitiveType.I32));
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
                graph.SetVarType("a", ArrayOf.Create(PrimitiveType.I32));
                graph.SetVar("a", 0);
                graph.SetVar("b", 1);
                graph.SetConcatCall(0, 1, 2);
                graph.SetDef("y", 2);
                var result = graph.Solve();
       }
    }
}
