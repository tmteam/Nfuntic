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
            GetEx();
            Console.ReadLine();
        }

        static void ConcreteGetEx()
        {
            //          2  0,1
            //y:char = get(x,0) 
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetConst(1, PrimitiveType.I32);
            graph.SetArrGetCall(0, 1, 2);
            graph.SetVarType("y", PrimitiveType.Char);
            graph.SetDef("y", 2);
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
        static void GetEx()
        {
            //     2  0,1
            //y = get(x,0)
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetConst(1, PrimitiveType.I32);
            graph.SetArrGetCall(0, 1, 2);
            graph.SetDef("y", 2);
            var result = graph.Solve();

        }
    }
}
