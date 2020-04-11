using System;
using System.Collections.Generic;
using System.Text;
using nfun.Ti4;
using NUnit.Framework;

namespace nfun.ti4.tests.Arrays
{
    public class ArrayFunctions
    {
        [Test(Description = "y = x[0]")]
        public void Get()
        {
            //     2  0,1
            //y = get(x,0) 
            var graph = new GraphBuilder();
            graph.SetVar("x",0);
            graph.SetConst(1, ConcreteType.I32);
            graph.SetArrGet(0, 1, 2);
            graph.SetDef("y",2);
            graph.Solve();
        }
    }
}
