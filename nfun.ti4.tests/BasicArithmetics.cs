using System;
using System.Collections.Generic;
using System.Text;
using nfun.Ti4;
using NUnit.Framework;

namespace nfun.ti4.tests
{
    class BasicArithmetics
    {
        [Test(Description = "y = 1 + 2 * x")]
        public void SolvingGenericWithSingleVar()
        {
            //node |    0 4 1 3 2
            //expr |y = 1 + 2 * x;

            var graph = new GraphBuilder();
            graph.SetIntConst(0, ConcreteType.U8);
            graph.SetIntConst(1, ConcreteType.U8);
            graph.SetVar("x", 2);
            graph.SetArith2(1,2,3);
            graph.SetArith2(0,3,4);
            graph.SetDef("y", 4);

            var result = graph.Solve();
            var generic = result.AssertAndGetSingleArithGeneric();
            result.AssertAreGenerics(generic, "x","y");
        }

        [TestCase]
        public void IncrementI64()
        {
            Console.WriteLine("y = x + 1i");

            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetConst(1, ConcreteType.I64);
            graph.SetArith2(0, 1, 2);
            graph.SetDef("y", 2);

            var result = graph.Solve();

            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.I64, "x","y");
        }

        [TestCase]
        public void IncrementU64WithStrictInputType()
        {
            Console.WriteLine("x:uint64; y = x + 1");

            var graph = new GraphBuilder();
            graph.SetVarType("x", ConcreteType.U64);
            graph.SetVar("x", 0);
            graph.SetIntConst(1, ConcreteType.U8);
            graph.SetArith2(0, 1, 2);
            graph.SetDef("y", 2);

            var result = graph.Solve();

            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.U64, "x","y");
        }
        [TestCase]
        public void IncrementU32WithStrictOutputType()
        {
            Console.WriteLine("y:u32 = x + 1");

            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetIntConst(1, ConcreteType.U8);
            graph.SetArith2(0, 1, 2);
            graph.SetVarType("y", ConcreteType.U32);
            graph.SetDef("y", 2);

            var result = graph.Solve();

            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.U32, "x","y");
        }

        [TestCase]
        public void GenericIncrement()
        {
            Console.WriteLine("y = x + 1");

            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetIntConst(1, ConcreteType.U8);
            graph.SetArith2(0, 1, 2);
            graph.SetDef("y", 2);

            var result = graph.Solve();

            var genericNode = result.AssertAndGetSingleArithGeneric();
            result.AssertAreGenerics(genericNode, "x", "y");
        }

        
        [Test]
        public void StrictOnEquationArithmetics()
        {
            Console.WriteLine("x= 10i;   a = x*y + 10-x");

            var graph = new GraphBuilder();
            graph.SetConst(0, ConcreteType.I32);
            graph.SetDef("x", 0);

            graph.SetVar("x", 1);
            graph.SetVar("y", 2);
            graph.SetArith2(1, 2, 3);
            graph.SetIntConst(4, ConcreteType.U8);
            graph.SetArith2(3, 4, 5);
            graph.SetVar("x", 6);
            graph.SetArith2(5, 6, 7);
            graph.SetDef("a", 7);


            var result = graph.Solve();

            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.I32, "x","y","a");
        }

        [Test]
        public void GenericOneEquatopmArithmetics()
        {
            Console.WriteLine("x= 10;   a = x*y + 10-x");

            var graph = new GraphBuilder();
            graph.SetIntConst(0, ConcreteType.U8);
            graph.SetDef("x", 0);

            graph.SetVar("x", 1);
            graph.SetVar("y", 2);
            graph.SetArith2(1, 2, 3);
            graph.SetIntConst(4, ConcreteType.U8);
            graph.SetArith2(3, 4, 5);
            graph.SetVar("x", 6);
            graph.SetArith2(5, 6, 7);
            graph.SetDef("a", 7);

            var result = graph.Solve();

            var genericNode = result.AssertAndGetSingleArithGeneric();
            result.AssertAreGenerics(genericNode,"x","y","a");
        }

        
        [Test]
        public void GenericTwoEquationsArithmetic()
        {
            Console.WriteLine("a = x*y + 10-x; b = r*x + 10-r");

            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetVar("y", 1);
            graph.SetArith2(0, 1, 2);
            graph.SetIntConst(3, ConcreteType.U8);
            graph.SetArith2(2, 3, 4);
            graph.SetVar("x", 5);
            graph.SetArith2(4, 5, 6);
            graph.SetDef("a", 6);

            graph.SetVar("r", 7);
            graph.SetVar("x", 8);
            graph.SetArith2(7, 8, 9);
            graph.SetIntConst(10, ConcreteType.U8);
            graph.SetArith2(9, 10, 11);
            graph.SetVar("r", 12);
            graph.SetArith2(11, 12, 13);
            graph.SetDef("b", 13);

            var result = graph.Solve();

            var genericNode = result.AssertAndGetSingleArithGeneric();
            result.AssertAreGenerics(genericNode, "x","y","a","b");
        }

        [Test]
        public void InputRepeats_simpleGeneric()
        {
            //node |3   0 2 1 
            //expr |y = x + x 

            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetVar("x", 1);
            graph.SetArith2(0,1,2);
            graph.SetDef("y", 2);
            var result = graph.Solve();

            var generic = result.AssertAndGetSingleArithGeneric();
            result.AssertAreGenerics(generic, "x","y");
        }

        [Test]
        //Есть два пути решения. С одной стороны мы можем обосновано сказать что b это дженерик
        //Но по логике - так как этот дженерик не участвует в выходе - нам нету смысла его держать дженериком
        //и мы можем сказать что это чистый риал. Нипонятно
        // ToDo
        public void UpcastArgTypeThatIsAfter_EquationSolved()
        {
            //     0 2 1       3 
            // y = a / b;  a = 1i

            var graph = new GraphBuilder();
            graph.SetVar("a", 0);
            graph.SetVar("b", 1);
            graph.SetCall(ConcreteType.Real, 0, 1, 2);
            graph.SetDef("y",2);
            graph.SetConst(3, ConcreteType.I32);
            graph.SetDef("a", 3);

            

            var result = graph.Solve();
            
            //Assert.AreEqual(0,result.GenericsCount);
            result.AssertNamed(ConcreteType.I32, "a");
            //Assert.AreEqual(ConcreteType.Real, result.GetVariable("b"));
            result.AssertNamed(ConcreteType.Real, "y");
        }

        [Test]
        //todo
        public void UpcastArgTypeThatIsBefore_EquationSolved()
        {
            //        0       1 3 2
            // // a = 1i; y = a / b;   

            var graph = new GraphBuilder();

            graph.SetConst(0, ConcreteType.I32);
            graph.SetDef("a", 0);


            graph.SetVar("a", 1);
            graph.SetVar("b", 2);
            graph.SetCall(ConcreteType.Real, 1, 2, 3);
            graph.SetDef("y", 3);

            var result = graph.Solve();

            //Assert.AreEqual(0,result.GenericsCount);
            result.AssertNamed(ConcreteType.I32, "a");
            //Assert.AreEqual(ConcreteType.Real, result.GetVariable("b"));
            result.AssertNamed(ConcreteType.Real, "y");
        }
        [Test]
        public void UpcastArgType_ArithmOp_EquationSolved()
        {
            //        0        1 3 2       4
            // // a = 1.0; y = a + b;  b = 1i

            var graph = new GraphBuilder();
            graph.SetConst(0, ConcreteType.Real);
            graph.SetDef("a", 0);

            graph.SetVar("a", 1);
            graph.SetVar("b", 2);
            graph.SetArith2(1, 2, 3);
            graph.SetDef("y", 3);


            graph.SetConst(4, ConcreteType.I32);
            graph.SetDef("b", 4);

            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.Real, "a");
            result.AssertNamed(ConcreteType.Real, "y");
            result.AssertNamed(ConcreteType.I32, "b");
        }
        [Test]
        public void TwoTypesAreLong_ItsSumIsLong()
        {
            //    0       1       2 4 3
            //a = 1l; b = 1l; x = a + b

            var graph = new GraphBuilder();
            graph.SetConst(0, ConcreteType.I64);
            graph.SetDef("a",0);

            graph.SetConst(1, ConcreteType.I64);
            graph.SetDef("b", 1);

            graph.SetVar("a", 2);
            graph.SetDef("b", 3);
            graph.SetArith2(2,3,4);
            graph.SetDef("x", 4);

            var result = graph.Solve();

            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.I64, "x","a","b");
        }

        [Test]
        public void MultipleAncestors_EquationSolved()
        {
            //      0         1        2  4  3
            //y1  = y0;  y2 = y1; y3 = y2 * 2i

            var graph = new GraphBuilder();
            graph.SetVar("y0",0);
            graph.SetDef("y1",0);

            graph.SetVar("y1", 1);
            graph.SetDef("y2", 1);

            graph.SetVar("y2", 2);
            graph.SetConst(3, ConcreteType.I32);
            graph.SetArith2(2,3,4);
            graph.SetDef("y3",4);

            var result = graph.Solve();

            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.I32, "y0","y1","y2","y3");
        }

        [Test]
        public void ReverseMultipleAncestors_EquationSolved()
        {
            //      0          1        2 4 3
            //y1  = y0;  y2 = y1; y3 = y0 * 2i
            var graph = new GraphBuilder();
            graph.SetVar("y0", 0);
            graph.SetDef("y1", 0);

            graph.SetVar("y1", 1);
            graph.SetDef("y2", 1);

            graph.SetVar("y0", 2);
            graph.SetConst(3, ConcreteType.I32);
            graph.SetArith2(2, 3, 4);
            graph.SetDef("y3", 4);

            var result = graph.Solve();

            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.I32, "y0", "y1", "y2", "y3");
        }
    }
}
