using System;
using Microsoft.Win32.SafeHandles;

namespace nfun.Ti4
{
    public class SolvingConstrains
    {
        public SolvingConstrains(PrimitiveType desc = null, PrimitiveType anc = null)
        {
            Descedant = desc;
            Ancestor = anc;
        }

        public SolvingConstrains GetCopy()
        {
            var result = new SolvingConstrains(Descedant, Ancestor)
            {
                IsComparable = this.IsComparable,
                PreferedType = this.PreferedType
            };
            return result;
        }
        public bool Fits(PrimitiveType primitive)
        {
            if (HasAncestor)
            {
                if (!primitive.CanBeImplicitlyConvertedTo(Ancestor))
                    return false;
            }

            if (HasDescendant)
            {
                if (!Descedant.CanBeImplicitlyConvertedTo(primitive))
                    return false;
            }

            if (IsComparable && !primitive.IsComparable)
                return false;
            return true;
        }

        public PrimitiveType Ancestor { get; private set; }
        public PrimitiveType Descedant { get; private set; }

        public bool HasAncestor => Ancestor!=null;
        public bool HasDescendant => Descedant!=null;

        public bool TryAddAncestor(PrimitiveType type)
        {
            if (type == null)
                return true;

            if (Ancestor == null)
                Ancestor = type;
            else
            {
                var res = Ancestor.GetFirstCommonDescendantOrNull(type);
                if (res == null)
                    return false;
                Ancestor = res;
            }

            return true;
        }
        public void AddAncestor(PrimitiveType type)
        {
            if(!TryAddAncestor(type))
                throw new InvalidOperationException();
        }

        public void AddDescedant(PrimitiveType type)
        {
            if(type==null)
                return;

            if (Descedant == null)
                Descedant = type;
            else
                Descedant = Descedant.GetLastCommonAncestor(type);
        }
        public PrimitiveType PreferedType { get; set; }
        public bool IsComparable { get; set; }
        public bool NoConstrains => !HasDescendant && !HasAncestor && !IsComparable;

        public object MergeOrNull(SolvingConstrains constrains)
        {
            var result = new SolvingConstrains(Descedant,Ancestor)
            {
                IsComparable = this.IsComparable || constrains.IsComparable
            };
            result.AddDescedant(constrains.Descedant);

            if (!result.TryAddAncestor(constrains.Ancestor))
                return null;

            if (result.HasAncestor && result.HasDescendant)
            {
                var anc = result.Ancestor;
                var des = result.Descedant;
                if (anc.Equals(des))
                {
                    if (result.IsComparable && !anc.IsComparable)
                        return null;
                    return anc;
                }
                if (!des.CanBeImplicitlyConvertedTo(anc))
                    return null;
            }

            return result;
        }
       
        public override string ToString() => $"[{Descedant}..{Ancestor}]";

        public object GetOptimizedOrThrow()
        {
            if (IsComparable)
            {
                if (Descedant != null)
                {
                    if (Descedant.Equals(PrimitiveType.Char))
                        return PrimitiveType.Char;
                    
                    if (Descedant.IsNumeric)
                    {
                        if(!TryAddAncestor(PrimitiveType.Real))
                            throw new InvalidOperationException();
                    }
                    else
                        throw new InvalidOperationException("Types cannot be compared");
                }
            }

            if (HasAncestor && HasDescendant)
            {
                if(Ancestor.Equals(Descedant))
                    return Ancestor;
                if (!Descedant.CanBeImplicitlyConvertedTo(Ancestor))
                    throw new InvalidOperationException();
            }

            if (Descedant?.Equals(PrimitiveType.Any)==true)
                return PrimitiveType.Any;

            return this;
        }
    }
}