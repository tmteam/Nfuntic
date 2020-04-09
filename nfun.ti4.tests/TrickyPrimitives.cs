using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using nfun.Ti4;
using NUnit.Framework;

namespace nfun.ti4.tests
{
    class TrickyPrimitives
    {
        [Test(Description = "y = isNan(1) ")]
        [Ignore("Обобщенная входная константа")]
        public void SimpleConcreteFunctionWithConstant()
        {
            //node |    1     0
            //expr |y = isNan(1) 
            var graph = new GraphBuilder();
            graph.SetIntConst(0, ConcreteType.U8);
            graph.SetCall(new []{ConcreteType.Real, ConcreteType.Bool}, new []{0,1});
            graph.SetDef("y", 1);
            var result = graph.Solve();

            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.Bool, "y");
        }

        [Test(Description = "y = isNan(x) ")]
        [Ignore("Обобщенный вход без выхода")]
        public void SimpleConcreteFunctionWithVariable()
        {
            //node |    1     0
            //expr |y = isNan(x) 
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetCall(new[] { ConcreteType.Real, ConcreteType.Bool }, new[] { 0, 1 });
            graph.SetDef("y", 1);
            var result = graph.Solve();

            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.Real, "x");
            result.AssertNamed(ConcreteType.Bool, "y");
        }

        [Test(Description = "x:int; y = isNan(x) ")]
        [Ignore("Обобщенный вход без выхода")]

        public void SimpleConcreteFunctionWithVariableOfConcreteType()
        {
            //node |           1     0
            //expr |x:int; y = isNan(x) 
            var graph = new GraphBuilder();
            graph.SetVarType("x", ConcreteType.I32);
            graph.SetVar("x", 0);
            graph.SetCall(new[] { ConcreteType.Real, ConcreteType.Bool }, new[] { 0, 1 });
            graph.SetDef("y", 1);
            var result = graph.Solve();

            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.Real, "x");
            result.AssertNamed(ConcreteType.Bool, "y");
        }
        [Test(Description = "y = isNan(x); z = ~x")]
        [Ignore("Обобщенный вход без выхода")]

        public void SimpleConcreteFunctionWithVariableThatLimitisAfterwards()
        {
            //node |    1     0       3        2
            //expr |y = isNan(x); z = isMaxInt(x) 
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetCall(new[] { ConcreteType.Real, ConcreteType.Bool }, new[] { 0, 1 });
            graph.SetDef("y", 1);

            graph.SetVar("x",2);
            graph.SetCall(new []{ConcreteType.I32, ConcreteType.Bool}, new []{2,3});
            graph.SetDef("z", 3);

            var result = graph.Solve();

            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.I32, "x");
            result.AssertNamed(ConcreteType.Bool, "y","z");
        }

        [Test(Description = "y = x ")]
        public void OutputEqualsInput_simpleGeneric()
        {
            //node |1   0
            //expr |y = x 
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetDef("y", 0);
            var result = graph.Solve();

            var generic = result.AssertAndGetSingleGeneric(null, null, false);
            result.AssertAreGenerics(generic, "x", "y");
        }

        [Test(Description = "y = x; | y2 = x2")]
        public void TwoSimpleGenerics()
        {
            //node |     0  |       1
            //expr s|y = x; | y2 = x2
            
            var graph = new GraphBuilder();
            graph.SetVar("x", 0);
            graph.SetDef("y", 0);

            graph.SetVar("x2", 1);
            graph.SetDef("y2", 1);

            var result = graph.Solve();

            Assert.AreEqual(2, result.GenericsCount);

            var generics = result.Generics.ToArray();

            generics[0].AssertGenericType(null, null, false);
            generics[1].AssertGenericType(null, null, false);

            var yRes = result.GetVariableNode("y").GetNonReference();
            var y2Res = result.GetVariableNode("y2").GetNonReference();
            CollectionAssert.AreEquivalent(generics, new[]{y2Res, yRes});

            var xRes = result.GetVariableNode("x").GetNonReference();
            var x2Res = result.GetVariableNode("x2").GetNonReference();
            CollectionAssert.AreEquivalent(generics, new[] { x2Res, xRes });

        }

        [Test]
        [Ignore("Обобщенная константа без выхода")]
        public void LimitCall_ComplexEquations_TypesSolved()
        {
            //     0 2 1      3 5  4      6 8 7
            // r = x + y; i = y << 2; x = 3 / 2
            var graph = new GraphBuilder();

            graph.SetVar("x", 0);
            graph.SetVar("y", 1);
            graph.SetArith(0,1,2);
            graph.SetDef("r",2);

            graph.SetVar("y", 3);
            graph.SetIntConst(4, ConcreteType.U8);
            graph.SetBitShift(3, 4, 5);
            graph.SetDef("i", 5);

            graph.SetIntConst(6, ConcreteType.U8);
            graph.SetIntConst(7, ConcreteType.U8);
            graph.SetCall(ConcreteType.Real, 6,7,8);
            graph.SetDef("x", 8);

            var result = graph.Solve();
            result.AssertNamed(ConcreteType.Real, "x", "r");
            var generic = result.AssertAndGetSingleGeneric(ConcreteType.U24, ConcreteType.I96);

            result.AssertAreGenerics(generic, "y","i");
        }

        [Test]
        public void SummReducecByBitShift_AllTypesAreInt()
        {
            //  0 2 1  4 3
            //( x + y )<<3

            var graph = new GraphBuilder();

            graph.SetVar("x", 0);
            graph.SetVar("y", 1);
            graph.SetArith(0, 1, 2);

            graph.SetIntConst(3, ConcreteType.U8);

            graph.SetBitShift(2, 3, 4);
            graph.SetDef("out", 4);

            var result = graph.Solve();
            var generic = result.AssertAndGetSingleGeneric(ConcreteType.U24, ConcreteType.I96);

            result.AssertAreGenerics(generic, "x", "y", "out");
        }


        [Test]
        public void TypeSpecified_PutHighterType_EquationSOlved()
        {
            //         1    0  
            //a:real;  a = 1:int32
            var graph = new GraphBuilder();
            graph.SetVarType("a", ConcreteType.Real);
            graph.SetConst(0, ConcreteType.I32);
            graph.SetDef("a",0);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.Real,"a" );
        }

        [Test]
        public void TypeLimitSet_ThanChangedToLower_LowerLimitAccepted()
        {
            //    0            1
            //a = 1:int;  a = 1.0:int64
            var graph = new GraphBuilder();
            graph.SetConst(0, ConcreteType.I32);
            graph.SetDef("a", 0);
            graph.SetConst(1, ConcreteType.I64);
            graph.SetDef("a", 1);

            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.I64, "a");
        }

        [Test]
        public void TypeLimitSet_ThanChangedToHigher_LowerLimitAccepted()
        {
            //1   0          3   2
            //a = 1:int64;  a = 1.0:int32

            var graph = new GraphBuilder();
            graph.SetConst(0, ConcreteType.I64);
            graph.SetDef("a", 0);
            graph.SetConst(1, ConcreteType.I32);
            graph.SetDef("a", 1);

            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.I64, "a");
        }
        [Test]
        public void OutEqualsToItself_SingleGenericFound()
        {
            //    0
            //y = y
            var graph = new GraphBuilder();
            graph.SetVar("y",0);
            graph.SetDef("y",0);
            var result = graph.Solve();
            var generic = result.AssertAndGetSingleGeneric(null, null, false);
            result.AssertAreGenerics(generic,"y");
        }

        [Test]
        public void OutEqualsToItself_TypeSpecified_EquationSolved()
        {
            //y:bool; y = y
            var graph = new GraphBuilder();
            graph.SetVarType("y", ConcreteType.Bool);
            graph.SetVar("y", 0);
            graph.SetDef("y", 0);
            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.Bool, "y");
        }

        [Test]
        public void OutEqualsToItself_TypeLimitedAfter_EquationSolved()
        {
            //y = y; y =1
            var graph = new GraphBuilder();
            graph.SetVar("y", 0);
            graph.SetDef("y", 0);
            graph.SetIntConst(1, ConcreteType.U8);
            graph.SetDef("y", 1);

            var result = graph.Solve();
            var generic = result.AssertAndGetSingleGeneric(ConcreteType.U8, ConcreteType.Real);
            result.AssertAreGenerics(generic,"y");
        }
        [Test]
        public void OutEqualsToItself_TypeLimitedBefore_EquationSolved()
        {
            //y = 1; y =y
            var graph = new GraphBuilder();
            graph.SetIntConst(0, ConcreteType.U8);
            graph.SetDef("y", 0);
            graph.SetVar("y", 1);
            graph.SetDef("y", 1);

            var result = graph.Solve();
            var generic = result.AssertAndGetSingleGeneric(ConcreteType.U8, ConcreteType.Real);
            result.AssertAreGenerics(generic, "y");
        }

        [Test]
        public void CircularDependencies_SingleGenericFound()
        {
            //    0      1      2
            //a = b; b = c; c = a
            var graph = new GraphBuilder();
            graph.SetVar("b", 0);
            graph.SetDef("a", 0);

            graph.SetVar("c", 1);
            graph.SetDef("b", 1);

            graph.SetVar("a", 2);
            graph.SetDef("c", 2);

            var result = graph.Solve();
            var generic = result.AssertAndGetSingleGeneric(null, null);
            result.AssertAreGenerics(generic, "a","b","c");
        }

        [Test]
        public void CircularDependencies_AllTypesSpecified_EquationSolved()
        {
            //    0      1      2
            //c:bool; a = b; b = c; c = a
            var graph = new GraphBuilder();
            graph.SetVarType("c",ConcreteType.Bool);
            graph.SetVar("b", 0);
            graph.SetDef("a", 0);

            graph.SetVar("c", 1);
            graph.SetDef("b", 1);

            graph.SetVar("a", 2);
            graph.SetDef("c", 2);

            var result = graph.Solve();
            result.AssertNoGenerics();
            result.AssertNamed(ConcreteType.Bool,"a","b","c");
        }

    }
}
