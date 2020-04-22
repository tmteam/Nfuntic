﻿using System;
using System.Collections.Generic;
using System.Text;
using nfun.Ti4;
using NUnit.Framework;

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
            var res = SolvingFunctions.GetMergedState(PrimitiveType.I32, new SolvingConstrains());
            Assert.AreEqual(res, PrimitiveType.I32);
        }

        [Test]
        public void GetMergedState_EmptyConstrainsAndPrimitive()
        {
            var res = SolvingFunctions.GetMergedState(new SolvingConstrains(), PrimitiveType.I32);
            Assert.AreEqual(res, PrimitiveType.I32);
        }
        [Test]
        public void GetMergedState_PrimitiveAndConstrainsThatFit()
        {
            var res = SolvingFunctions.GetMergedState(PrimitiveType.I32, new SolvingConstrains(PrimitiveType.U24, PrimitiveType.I48));
            Assert.AreEqual(res, PrimitiveType.I32);
        }
        [Test]
        public void GetMergedState_ConstrainsThatFitAndPrimitive()
        {
            var res = SolvingFunctions.GetMergedState(new SolvingConstrains(PrimitiveType.U24, PrimitiveType.I48), PrimitiveType.I32);
            Assert.AreEqual(res, PrimitiveType.I32);
        }
        [Test]
        public void GetMergedState_TwoSameConcreteArrays()
        {
            var res = SolvingFunctions.GetMergedState(ArrayOf.Create(PrimitiveType.I32), ArrayOf.Create(PrimitiveType.I32));
            Assert.AreEqual(res, ArrayOf.Create(PrimitiveType.I32));
        }

        #region obviousFailed

        [Test]
        public void GetMergedState_PrimitiveAndConstrainsThatNotFit() 
            => AssertGetMergedStateThrows(PrimitiveType.I32, new SolvingConstrains(PrimitiveType.U24, PrimitiveType.U48));

        [Test]
        public void GetMergedState_TwoDifferentPrimitivesThrows() 
            => AssertGetMergedStateThrows(PrimitiveType.I32, PrimitiveType.Real);

        [Test]
        public void GetMergedState_TwoDifferentConcreteArraysThrows()
            => AssertGetMergedStateThrows(
                    stateA: ArrayOf.Create(PrimitiveType.I32), 
                    stateB: ArrayOf.Create(PrimitiveType.Real));
        #endregion

        void AssertGetMergedStateThrows(ISolvingState stateA, ISolvingState stateB)
        {
            Assert.Throws<InvalidOperationException>(
                () => SolvingFunctions.GetMergedState(stateA, stateB));
            Assert.Throws<InvalidOperationException>(
                () => SolvingFunctions.GetMergedState(stateB, stateA));
        }
    }
}
