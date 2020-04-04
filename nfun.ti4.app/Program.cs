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
            Trace3();
            Console.ReadLine();
        }

        private static void Trace1()
        {
            Console.WriteLine("y = x + 1");
            
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetIntConst(1, ConcreteType.U8);
            graph.SetArith(0, 1, 2);
            graph.SetDef("y", 2);

            SolveTypes(graph);
        }

        private static void Trace2()
        {
            Console.WriteLine("x = x + 1");

            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetIntConst(1, ConcreteType.U8);
            graph.SetArith(0, 1, 2);
            graph.SetDef("x", 2);
            SolveTypes(graph);

        }

        private static void Trace3()
        {
            Console.WriteLine("x= 10i;   a = x*y + 10-x");

            var graph = new GraphBuilder();
            graph.SetConst(0, ConcreteType.I32);
            graph.SetDef("x", 0);
            
            graph.SetVar("x", 1);
            graph.SetVar("y", 2);
            graph.SetArith(1, 2,3);
            graph.SetIntConst(4, ConcreteType.U8);
            graph.SetArith(3,4,5);
            graph.SetVar("x", 6);
            graph.SetArith(5,6,7);
            graph.SetDef("a", 7);
          
            SolveTypes(graph);

        }

        private static void Trace4()
        {
            Console.WriteLine("x= 10;   a = x*y + 10-x");

            var graph = new GraphBuilder();
            graph.SetIntConst(0, ConcreteType.U8);
            graph.SetDef("x", 0);

            graph.SetVar("x", 1);
            graph.SetVar("y", 2);
            graph.SetArith(1, 2, 3);
            graph.SetIntConst(4, ConcreteType.U8);
            graph.SetArith(3, 4, 5);
            graph.SetVar("x", 6);
            graph.SetArith(5, 6, 7);
            graph.SetDef("a", 7);

            SolveTypes(graph);

        }

        private static void SolveTypes(GraphBuilder graph)
        {
            graph.PrintTrace();
            Console.WriteLine();

            var sorted = graph.Toposort();
            Console.WriteLine();
            graph.PrintTrace();

            SolvingFunctions.SetUpwardsLimits(sorted);
            Console.WriteLine();
            Console.WriteLine("Set up");
            graph.PrintTrace();

            SolvingFunctions.SetDownwardsLimits(sorted);
            Console.WriteLine();
            Console.WriteLine("Set down");
            graph.PrintTrace();

            SolvingFunctions.DestructiveMergeAll(sorted.ToArray());

            Console.WriteLine();
            Console.WriteLine("DestructiveMerge");
            graph.PrintTrace();
        }
    }
}
