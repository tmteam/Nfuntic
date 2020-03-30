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
            Trace1();
            Console.ReadLine();

        }

        private static void Trace1()
        {
            Console.WriteLine("y = x + 1");
            
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetIntConst(1, SolvingNode.U8);
            graph.SetArith(0, 1, 2);
            graph.SetDef("y", 2);
            graph.PrintTrace();
        }
    }
}
