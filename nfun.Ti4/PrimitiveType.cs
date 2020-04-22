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
    }

    public enum ConvertPossibilities
    {
        Convertable,
        NotConvertable,
        NeedAdditionalInformation
    }
    public class PrimitiveType: IType, ISolvingState
    {
        private static PrimitiveType[] IntegerTypes;
        private static PrimitiveType[] UintTypes;

        static PrimitiveType()
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

        public PrimitiveType(PrimitiveTypeName name)
        {
            Name = name;
        }

        public PrimitiveTypeName Name { get; }

        public bool IsSolved => true;
        public bool IsNumeric => Name.HasFlag(PrimitiveTypeName._IsNumber);
        
        private int Layer => (int)((int)Name >>5 & 0b1111);

        public override string ToString() => Name.ToString();

        public static PrimitiveType Any { get; } = new PrimitiveType(PrimitiveTypeName.Any);
        public static PrimitiveType Bool { get; } = new PrimitiveType(PrimitiveTypeName.Bool);
        public static PrimitiveType Char { get; } = new PrimitiveType(PrimitiveTypeName.Char);
        public static PrimitiveType Real { get; } = new PrimitiveType(PrimitiveTypeName.Real);
        public static PrimitiveType I96 { get; } = new PrimitiveType(PrimitiveTypeName.I96);
        public static PrimitiveType I64 { get; } = new PrimitiveType(PrimitiveTypeName.I64);
        public static PrimitiveType I48 { get; } = new PrimitiveType(PrimitiveTypeName.I48);
        public static PrimitiveType I32 { get; } = new PrimitiveType(PrimitiveTypeName.I32);
        public static PrimitiveType I24 { get; } = new PrimitiveType(PrimitiveTypeName.I24);
        public static PrimitiveType I16 { get; } = new PrimitiveType(PrimitiveTypeName.I16);
        public static PrimitiveType U64 { get; } = new PrimitiveType(PrimitiveTypeName.U64);
        public static PrimitiveType U48 { get; } = new PrimitiveType(PrimitiveTypeName.U48);
        public static PrimitiveType U32 { get; } = new PrimitiveType(PrimitiveTypeName.U32);
        public static PrimitiveType U24 { get; } = new PrimitiveType(PrimitiveTypeName.U24);
        public static PrimitiveType U16 { get; } = new PrimitiveType(PrimitiveTypeName.U16);
        public static PrimitiveType U12 { get; } = new PrimitiveType(PrimitiveTypeName.U12);
        public static PrimitiveType U8 { get; } = new PrimitiveType(PrimitiveTypeName.U8);
        public bool IsComparable => IsNumeric || Name == PrimitiveTypeName.Char;

        public bool CanBeImplicitlyConvertedTo(PrimitiveType type)
        {
            if (type.Name == PrimitiveTypeName.Any)
                return true;
            if (this.Equals(type))
                return true;
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

        public PrimitiveType GetFirstCommonDescendantOrNull(PrimitiveType otherType)
        {
            if (this.Equals(otherType))
                return this;

            if (otherType.CanBeImplicitlyConvertedTo(this))
                return otherType;
            if (this.CanBeImplicitlyConvertedTo(otherType))
                return this;
            
            if (!otherType.IsNumeric || !this.IsNumeric)
                return null;

            var intType = otherType;

            if (otherType.Name.HasFlag(PrimitiveTypeName._IsUint))
                intType = this;

            var layer = intType.Layer + 1;
            return UintTypes[layer-3];
        }
        public IType GetLastCommonAncestorOrNull(IType otherType)
        {
            var primitive = otherType as PrimitiveType;
            if (primitive == null)
                return Any;
            return GetLastCommonPrimitiveAncestor(primitive);
        }

        public PrimitiveType GetLastCommonPrimitiveAncestor(PrimitiveType otherType)
        {
            if (this.Equals(otherType))
                return this;
            
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

        public override bool Equals(object obj) => (obj as PrimitiveType)?.Name == Name;
    }
}
