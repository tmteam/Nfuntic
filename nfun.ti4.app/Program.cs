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
            //              2     0 1
            //a:int[]; y = concat(a,b) 
            var graph = new GraphBuilder();
            graph.SetVarType("a", ConcreteType.ArrayOf(ConcreteType.I32));
            graph.SetVar("a", 0);
            graph.SetVar("b", 1);
            graph.SetConcatCall(0, 1, 2);
            graph.SetDef("y", 2);
            graph.Solve();

            Console.ReadLine();
        }
    }
}
