using nfun.Ti4;
using NUnit.Framework;

namespace nfun.ti4.tests
{
    public class ConcreteTypeTest
    {
        [TestCase(PrimitiveTypeName.Real, PrimitiveTypeName.Real, PrimitiveTypeName.Real)]
        [TestCase(PrimitiveTypeName.Real, PrimitiveTypeName.Char, PrimitiveTypeName.Any)]
        [TestCase(PrimitiveTypeName.Any, PrimitiveTypeName.Real, PrimitiveTypeName.Any)]
        [TestCase(PrimitiveTypeName.U24, PrimitiveTypeName.I32, PrimitiveTypeName.I32)]
        [TestCase(PrimitiveTypeName.I48, PrimitiveTypeName.I32, PrimitiveTypeName.I48)]
        [TestCase(PrimitiveTypeName.U48, PrimitiveTypeName.I32, PrimitiveTypeName.I64)]
        [TestCase(PrimitiveTypeName.U48, PrimitiveTypeName.U32, PrimitiveTypeName.U48)]
        [TestCase(PrimitiveTypeName.U64, PrimitiveTypeName.I32, PrimitiveTypeName.I96)]
        [TestCase(PrimitiveTypeName.U48, PrimitiveTypeName.Real, PrimitiveTypeName.Real)]
        public void GetLastCommonAncestor(PrimitiveTypeName a, PrimitiveTypeName b, PrimitiveTypeName expected)
        {
            var result =  new ConcreteType(a).GetLastCommonAncestor(new ConcreteType(b)).Name;
            Assert.AreEqual( expected, result);
            var revresult = new ConcreteType(b).GetLastCommonAncestor(new ConcreteType(a)).Name;
            Assert.AreEqual(expected, revresult);
        }

        [TestCase(PrimitiveTypeName.Real, PrimitiveTypeName.Char)]
        [TestCase(PrimitiveTypeName.Real, PrimitiveTypeName.Bool)]
        [TestCase(PrimitiveTypeName.Bool, PrimitiveTypeName.Char)]
        public void GetFirstCommonDescedant_returnsNull(PrimitiveTypeName a, PrimitiveTypeName b)
        {
            var result = new ConcreteType(a).GetFirstCommonDescedantOrNull(new ConcreteType(b));
            Assert.IsNull(result);
            var revresult = new ConcreteType(b).GetFirstCommonDescedantOrNull(new ConcreteType(a));
            Assert.IsNull(revresult);
        }


        [TestCase(PrimitiveTypeName.Real, PrimitiveTypeName.Real, PrimitiveTypeName.Real)]
        [TestCase(PrimitiveTypeName.Any, PrimitiveTypeName.Real, PrimitiveTypeName.Real)]
        [TestCase(PrimitiveTypeName.U24, PrimitiveTypeName.I32, PrimitiveTypeName.U24)]
        [TestCase(PrimitiveTypeName.I48, PrimitiveTypeName.I32, PrimitiveTypeName.I32)]
        [TestCase(PrimitiveTypeName.U48, PrimitiveTypeName.I32, PrimitiveTypeName.U24)]
        [TestCase(PrimitiveTypeName.U48, PrimitiveTypeName.U32, PrimitiveTypeName.U32)]
        [TestCase(PrimitiveTypeName.U64, PrimitiveTypeName.I32, PrimitiveTypeName.U24)]
        [TestCase(PrimitiveTypeName.U48, PrimitiveTypeName.Real, PrimitiveTypeName.U48)]
        [TestCase(PrimitiveTypeName.I16, PrimitiveTypeName.U16, PrimitiveTypeName.U12)]
        [TestCase(PrimitiveTypeName.I16, PrimitiveTypeName.U12, PrimitiveTypeName.U12)]
        [TestCase(PrimitiveTypeName.U8, PrimitiveTypeName.U8, PrimitiveTypeName.U8)]



        public void GetFirstCommonDescedant(PrimitiveTypeName a, PrimitiveTypeName b, PrimitiveTypeName expected)
        {
            var result = new ConcreteType(a).GetFirstCommonDescedantOrNull(new ConcreteType(b)).Name;
            Assert.AreEqual(expected, result);
            var revresult = new ConcreteType(b).GetFirstCommonDescedantOrNull(new ConcreteType(a)).Name;
            Assert.AreEqual(expected, revresult);
        }

        [TestCase(PrimitiveTypeName.Real, PrimitiveTypeName.Real)]
        [TestCase(PrimitiveTypeName.I32, PrimitiveTypeName.Real)]
        [TestCase(PrimitiveTypeName.U24, PrimitiveTypeName.Real)]
        [TestCase(PrimitiveTypeName.I64, PrimitiveTypeName.Real)]
        [TestCase(PrimitiveTypeName.U64, PrimitiveTypeName.Real)]
        [TestCase(PrimitiveTypeName.U8, PrimitiveTypeName.Real)]
        [TestCase(PrimitiveTypeName.Real, PrimitiveTypeName.Any)]
        [TestCase(PrimitiveTypeName.I32, PrimitiveTypeName.I96)]
        [TestCase(PrimitiveTypeName.U24, PrimitiveTypeName.I96)]
        [TestCase(PrimitiveTypeName.I64, PrimitiveTypeName.I96)]
        [TestCase(PrimitiveTypeName.U64, PrimitiveTypeName.I96)]
        [TestCase(PrimitiveTypeName.U8, PrimitiveTypeName.I96)]
        [TestCase(PrimitiveTypeName.U24, PrimitiveTypeName.U64)]
        [TestCase(PrimitiveTypeName.U64, PrimitiveTypeName.U64)]
        [TestCase(PrimitiveTypeName.U8, PrimitiveTypeName.U64)]
        [TestCase(PrimitiveTypeName.Char, PrimitiveTypeName.Any)]

        public void CanBeImplicitlyConverted_returnsTrue(PrimitiveTypeName from, PrimitiveTypeName to)
        {
            Assert.IsTrue(new ConcreteType(from).CanBeImplicitlyConvertedTo(new ConcreteType(to)));
        }
        [TestCase(PrimitiveTypeName.Real, PrimitiveTypeName.U48)]
        [TestCase(PrimitiveTypeName.I32, PrimitiveTypeName.U64)]
        [TestCase(PrimitiveTypeName.U24, PrimitiveTypeName.I16)]
        [TestCase(PrimitiveTypeName.I64, PrimitiveTypeName.I32)]
        [TestCase(PrimitiveTypeName.Real, PrimitiveTypeName.I64)]
        [TestCase(PrimitiveTypeName.I96, PrimitiveTypeName.U48)]
        [TestCase(PrimitiveTypeName.Any, PrimitiveTypeName.Real)]
        [TestCase(PrimitiveTypeName.I32, PrimitiveTypeName.U12)]
        [TestCase(PrimitiveTypeName.U24, PrimitiveTypeName.U12)]
        [TestCase(PrimitiveTypeName.I64, PrimitiveTypeName.I24)]
        [TestCase(PrimitiveTypeName.U64, PrimitiveTypeName.I16)]
        [TestCase(PrimitiveTypeName.Char, PrimitiveTypeName.Bool)]
        public void CanBeImplicitlyConverted_returnsFalse(PrimitiveTypeName from, PrimitiveTypeName to)
        {
            Assert.IsFalse(new ConcreteType(from).CanBeImplicitlyConvertedTo(new ConcreteType(to)));
        }
    }
}