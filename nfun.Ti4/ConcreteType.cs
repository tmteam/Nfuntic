using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        I96 =  _IsPrimitive | _IsNumber | 2 << 5 | _isAbstract,
        I64 = _IsPrimitive | _IsNumber  | 3 << 5,
        I48 = _IsPrimitive | _IsNumber  | 4 << 5 | _isAbstract,
        I32 = _IsPrimitive | _IsNumber  | 5 << 5,
        I24 = _IsPrimitive | _IsNumber  | 6 << 5 | _isAbstract,
        I16 = _IsPrimitive | _IsNumber  | 7 << 5,

        U64 = _IsPrimitive | _IsNumber | _IsUint  | 3 << 5,
        U48 = _IsPrimitive | _IsNumber | _IsUint  | 4 << 5 | _isAbstract,
        U32  = _IsPrimitive | _IsNumber | _IsUint | 5 << 5,
        U24 = _IsPrimitive | _IsNumber | _IsUint  | 6 << 5 | _isAbstract,
        U16 = _IsPrimitive | _IsNumber | _IsUint  | 7 << 5,
        U12 = _IsPrimitive | _IsNumber | _IsUint  | 8 << 5 | _isAbstract,
        U8 = _IsPrimitive | _IsNumber | _IsUint   | 9 << 5
    }

    public class ConcreteType
    {
        public ConcreteType(PrimitiveTypeName name)
        {
            Name = name;
        }

        public PrimitiveTypeName Name { get; }
        public bool IsPrimitive => true;
        public bool IsNumeric => Name >= PrimitiveTypeName.Real;

        private int Layer => (int)((int)Name & 0xb11110000);

        public bool CanBeImplicitlConvertedTo(ConcreteType type)
        {
            if (this.Name == PrimitiveTypeName.Any)
                return true;
            if (this.Name == type.Name)
                return true;
            if (!this.IsNumeric || !type.IsNumeric)
                return false;
            //So both are numbers
            if (this.Name == PrimitiveTypeName.Real)
                return true;
            if (this.Layer >= type.Layer)
                return false;
            if (this.Name.HasFlag(PrimitiveTypeName._IsUint))
                return type.Name.HasFlag(PrimitiveTypeName._IsUint);
            return true;
        }
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


    }
}
