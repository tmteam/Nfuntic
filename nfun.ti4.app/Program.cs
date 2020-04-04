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
            Trace5();
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

        private static void Trace5()
        {
            Console.WriteLine("a = x*y + 10-x; b = r*x + 10-r");

            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetVar("y", 1);
            graph.SetArith(0, 1, 2);
            graph.SetIntConst(3, ConcreteType.U8);
            graph.SetArith(2, 3, 4);
            graph.SetVar("x", 5);
            graph.SetArith(4, 5, 6);
            graph.SetDef("a", 6);

            graph.SetVar("r", 7);
            graph.SetVar("x", 8);
            graph.SetArith(7, 8, 9);
            graph.SetIntConst(10, ConcreteType.U8);
            graph.SetArith(9, 10, 11);
            graph.SetVar("r", 12);
            graph.SetArith(11, 12, 13);
            graph.SetDef("b", 13);


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

            SolvingFunctions.DestructiveMergeAll(sorted);

            Console.WriteLine();
            Console.WriteLine("Destruct Down");
            graph.PrintTrace();

            Console.WriteLine("Finalize");
            var results = SolvingFunctions.FinalizeUp(sorted);
            
            Console.WriteLine($"Type variables: {results.TypeVariables.Length}");
            foreach (var typeVariable in results.TypeVariables)
                Console.WriteLine("    "+ typeVariable);

            Console.WriteLine($"Syntax node types: ");
            foreach (var syntaxNode in results.SyntaxNodes.Where(s=>s!=null))
                Console.WriteLine("    " + syntaxNode);

            Console.WriteLine($"Named node types: ");
            foreach (var namedNode in results.NamedNodes)
                Console.WriteLine("    " + namedNode);

        }
    }
}
