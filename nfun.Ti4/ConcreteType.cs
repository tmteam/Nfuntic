using System;

namespace nfun.Ti4
{
    public enum PrimitiveTypeName
    {
        Any = _IsPrimitive,

        _IsPrimitive = 1<<0,
        _IsNumber    = 1<<1,
        _IsUint      = 1<<2,
        _isAbstract  = 1<<3,
        
        Char = _IsPrimitive | 1<<5 | 1<<9,
        Bool = _IsPrimitive | 1<<5 | 2<<9,

        Real = _IsPrimitive | _IsNumber | 1 << 5,
        I96  = _IsPrimitive | _IsNumber | 2 << 5 | _isAbstract,
        I64  = _IsPrimitive | _IsNumber | 3 << 5,
        I48  = _IsPrimitive | _IsNumber | 4 << 5 | _isAbstract,
        I32  = _IsPrimitive | _IsNumber | 5 << 5,
        I24  = _IsPrimitive | _IsNumber | 6 << 5 | _isAbstract,
        I16  = _IsPrimitive | _IsNumber | 7 << 5,

        U64  = _IsPrimitive | _IsNumber | _IsUint | 3 << 5,
        U48  = _IsPrimitive | _IsNumber | _IsUint | 4 << 5 | _isAbstract,
        U32  = _IsPrimitive | _IsNumber | _IsUint | 5 << 5,
        U24  = _IsPrimitive | _IsNumber | _IsUint | 6 << 5 | _isAbstract,
        U16  = _IsPrimitive | _IsNumber | _IsUint | 7 << 5,
        U12  = _IsPrimitive | _IsNumber | _IsUint | 8 << 5 | _isAbstract,
        U8   = _IsPrimitive | _IsNumber | _IsUint | 9 << 5,
        ArrayOf = 1<<5| 3 << 9
    }

    public class ConcreteArrayType: ConcreteType
    {
        public ConcreteType ElementType { get; }

        public ConcreteArrayType(ConcreteType elementType):base(PrimitiveTypeName.ArrayOf)
        {
            ElementType = elementType;
        }

        public override string ToString() => $"{ElementType}[]";
    }
    public class ConcreteType
    {
        private static ConcreteType[] IntegerTypes;
        private static ConcreteType[] UintTypes;

        static ConcreteType()
        {
            UintTypes = new[]
            {

                U64,
                U48,
                U32,
                U24,
                U16,
                U12,
                U8
            };
            IntegerTypes = new[]
            {
                Real,
                I96,
                I64,
                I48,
                I32,
                I24,
                I16
            };
        }

        public ConcreteType(PrimitiveTypeName name)
        {
            Name = name;
        }

        public PrimitiveTypeName Name { get; }
        public bool IsPrimitive => Name.HasFlag(PrimitiveTypeName._IsPrimitive);
        public bool IsNumeric => Name.HasFlag(PrimitiveTypeName._IsNumber);

        private int Layer => (int)((int)Name >>5 & 0b1111);

        public override string ToString() => Name.ToString();

        public static ConcreteArrayType ArrayOf(ConcreteType type) => new ConcreteArrayType(type);
        public static ConcreteType Any { get; } = new ConcreteType(PrimitiveTypeName.Any);
        public static ConcreteType Bool { get; } = new ConcreteType(PrimitiveTypeName.Bool);
        public static ConcreteType Char { get; } = new ConcreteType(PrimitiveTypeName.Char);
        public static ConcreteType Real { get; } = new ConcreteType(PrimitiveTypeName.Real);
        public static ConcreteType I96 { get; } = new ConcreteType(PrimitiveTypeName.I96);
        public static ConcreteType I64 { get; } = new ConcreteType(PrimitiveTypeName.I64);
        public static ConcreteType I48 { get; } = new ConcreteType(PrimitiveTypeName.I48);
        public static ConcreteType I32 { get; } = new ConcreteType(PrimitiveTypeName.I32);
        public static ConcreteType I24 { get; } = new ConcreteType(PrimitiveTypeName.I24);
        public static ConcreteType I16 { get; } = new ConcreteType(PrimitiveTypeName.I16);
        public static ConcreteType U64 { get; } = new ConcreteType(PrimitiveTypeName.U64);
        public static ConcreteType U48 { get; } = new ConcreteType(PrimitiveTypeName.U48);
        public static ConcreteType U32 { get; } = new ConcreteType(PrimitiveTypeName.U32);
        public static ConcreteType U24 { get; } = new ConcreteType(PrimitiveTypeName.U24);
        public static ConcreteType U16 { get; } = new ConcreteType(PrimitiveTypeName.U16);
        public static ConcreteType U12 { get; } = new ConcreteType(PrimitiveTypeName.U12);
        public static ConcreteType U8 { get; } = new ConcreteType(PrimitiveTypeName.U8);
        public bool IsComparable => IsNumeric || Name == PrimitiveTypeName.Char;


        public bool CanBeImplicitlyConvertedTo(ConcreteType type)
        {
            if (type.Name == PrimitiveTypeName.Any)
                return true;
            if (this.Equals(type))
                return true;
            if (this is ConcreteArrayType a1 && type is ConcreteArrayType a2)
                return a1.ElementType.CanBeImplicitlyConvertedTo(a2.ElementType);
            if (!this.IsNumeric || !type.IsNumeric)
                return false;
            //So both are numbers
            if (type.Name == PrimitiveTypeName.Real)
                return true;
            if (this.Layer <= type.Layer)
                return false;
            if (type.Name.HasFlag(PrimitiveTypeName._IsUint))
                return this.Name.HasFlag(PrimitiveTypeName._IsUint);
            return true;
        }

        public ConcreteType GetFirstCommonDescendantOrNull(ConcreteType otherType)
        {
            if (this.Equals(otherType))
                return this;

            if (otherType.CanBeImplicitlyConvertedTo(this))
                return otherType;
            if (this.CanBeImplicitlyConvertedTo(otherType))
                return this;
            if (this is ConcreteArrayType a1 && otherType is ConcreteArrayType a2)
            {
                var elementType = a1.ElementType.GetFirstCommonDescendantOrNull(a2.ElementType);
                if (elementType == null)
                    return null;
                return ArrayOf(elementType);
            }

            if (!otherType.IsNumeric || !this.IsNumeric)
                return null;

            var intType = otherType;

            if (otherType.Name.HasFlag(PrimitiveTypeName._IsUint))
                intType = this;

            var layer = intType.Layer + 1;
            return UintTypes[layer-3];
        }

        public ConcreteType GetLastCommonAncestor(ConcreteType otherType)
        {
            if (this.Equals(otherType))
                return this;
            if (this is ConcreteArrayType a1 && otherType is ConcreteArrayType a2)
            {
                var elementType = a1.GetLastCommonAncestor(a2);
                return ConcreteType.ArrayOf(elementType);
            }

            if (!otherType.IsNumeric || !this.IsNumeric)
                return Any;
            if (otherType.CanBeImplicitlyConvertedTo(this))
                return this;
            if (this.CanBeImplicitlyConvertedTo(otherType))
                return otherType;

            var uintType = this;
            if (otherType.Name.HasFlag(PrimitiveTypeName._IsUint))
                uintType = otherType;

            for (int i = uintType.Layer; i >= 1; i--)
            {
                if (uintType.CanBeImplicitlyConvertedTo(IntegerTypes[i]))
                    return IntegerTypes[i];
            }

            throw new InvalidOperationException();
        }

        public override bool Equals(object obj)
        {
            if (obj is ConcreteArrayType a1)
            {
                if (!(this is ConcreteArrayType a2))
                    return false;
                return a1.ElementType.Equals(a2.ElementType);
            }
            return (obj as ConcreteType)?.Name == Name;
        }
    }
}
