using System;
using System.Collections.Generic;
using System.Text;
using nfun.Ti4;
using NUnit.Framework;

namespace nfun.ti4.tests
{
    class Arrays
    {
        [Test]
        public void ArrayInit()
        {
            //    3 0 1 2 
            // y = [1,2,3]
            var graph = new GraphBuilder();
            graph.SetIntConst(0, ConcreteType.U8);
            graph.SetIntConst(1, ConcreteType.U8);
            graph.SetIntConst(2, ConcreteType.U8);
            graph.SetArrayInit(3, 0,1,2);
            graph.Solve();
        }
    }
}
