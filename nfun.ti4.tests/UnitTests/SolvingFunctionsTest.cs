using System;
using System.Collections.Generic;
using System.Text;
using nfun.Ti4;
using NUnit.Framework;
using Array = nfun.Ti4.Array;

namespace nfun.ti4.tests.UnitTests
{
    class SolvingFunctionsTest
    {
        [Test]
        public void GetMergedState_TwoSamePrimitives()
        {
            var res = SolvingFunctions.GetMergedState(PrimitiveType.I32, PrimitiveType.I32);
            Assert.AreEqual(res, PrimitiveType.I32);
        }

        [Test]
        public void GetMergedState_PrimitiveAndEmptyConstrains()
        {
            var res = SolvingFunctions.GetMergedState(PrimitiveType.I32, new Constrains());
            Assert.AreEqual(res, PrimitiveType.I32);
        }

        [Test]
        public void GetMergedState_EmptyConstrainsAndPrimitive()
        {
            var res = SolvingFunctions.GetMergedState(new Constrains(), PrimitiveType.I32);
            Assert.AreEqual(res, PrimitiveType.I32);
        }
        [Test]
        public void GetMergedState_PrimitiveAndConstrainsThatFit()
        {
            var res = SolvingFunctions.GetMergedState(PrimitiveType.I32, new Constrains(PrimitiveType.U24, PrimitiveType.I48));
            Assert.AreEqual(res, PrimitiveType.I32);
        }
        [Test]
        public void GetMergedState_ConstrainsThatFitAndPrimitive()
        {
            var res = SolvingFunctions.GetMergedState(new Constrains(PrimitiveType.U24, PrimitiveType.I48), PrimitiveType.I32);
            Assert.AreEqual(res, PrimitiveType.I32);
        }
        [Test]
        public void GetMergedState_TwoSameConcreteArrays()
        {
            var res = SolvingFunctions.GetMergedState(Array.Of(PrimitiveType.I32), Array.Of(PrimitiveType.I32));
            Assert.AreEqual(res, Array.Of(PrimitiveType.I32));
        }

        #region obviousFailed

        [Test]
        public void GetMergedState_PrimitiveAndConstrainsThatNotFit() 
            => AssertGetMergedStateThrows(PrimitiveType.I32, new Constrains(PrimitiveType.U24, PrimitiveType.U48));

        [Test]
        public void GetMergedState_TwoDifferentPrimitivesThrows() 
            => AssertGetMergedStateThrows(PrimitiveType.I32, PrimitiveType.Real);

        [Test]
        public void GetMergedState_TwoDifferentConcreteArraysThrows()
            => AssertGetMergedStateThrows(
                    stateA: Array.Of(PrimitiveType.I32), 
                    stateB: Array.Of(PrimitiveType.Real));
        #endregion

        void AssertGetMergedStateThrows(IState stateA, IState stateB)
        {
            Assert.Throws<InvalidOperationException>(
                () => SolvingFunctions.GetMergedState(stateA, stateB));
            Assert.Throws<InvalidOperationException>(
                () => SolvingFunctions.GetMergedState(stateB, stateA));
        }
    }
}
