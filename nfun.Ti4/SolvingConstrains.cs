using System;
using Microsoft.Win32.SafeHandles;

namespace nfun.Ti4
{
    public class SolvingConstrains: ISolvingState
    {
        public SolvingConstrains(IType desc = null, PrimitiveType anc = null)
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

        public bool Fits(IType type)
        {
            if (HasAncestor)
            {
                if (!type.CanBeImplicitlyConvertedTo(Ancestor))
                    return false;
            }

            if (type is PrimitiveType primitive)
            {
                if (HasDescendant)
                {
                    if (!Descedant.CanBeImplicitlyConvertedTo(primitive))
                        return false;
                }

                if (IsComparable && !primitive.IsComparable)
                    return false;
                return true;
            }
            else if (type is ArrayOf array)
            {
                if (IsComparable)
                    return false;
                if (!HasDescendant)
                    return true;
                if (!(Descedant is ArrayOf descArray))
                    return false;
                if (array.Element.Equals(descArray.Element))
                    return true;
                if (!array.IsSolved || !descArray.IsSolved)
                    return false;
            }
            throw new NotImplementedException();
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
        public IType Descedant { get; private set; }

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

        public void AddDescedant(IType type)
        {
            
            if(type==null)
                return;
            if(!type.IsSolved)
                return;

            if (Descedant == null)
                Descedant = type;
            else
            {
                var ancestor = Descedant.GetLastCommonAncestorOrNull(type);
                if(ancestor!=null)
                    Descedant = ancestor;
            }
        }
        public PrimitiveType PreferedType { get; set; }
        public bool IsComparable { get; set; }
        public bool NoConstrains => !HasDescendant && !HasAncestor && !IsComparable;

        public ISolvingState MergeOrNull(SolvingConstrains constrains)
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
       
        public override string ToString()
        {
            var res =  $"[{Descedant}..{Ancestor}]";
            if (IsComparable)
                res += "<>";
            if (PreferedType != null)
                res += PreferedType + "!";
            return res;
        }

        public ISolvingState GetOptimizedOrThrow()
        {
            if (IsComparable)
            {
                if (Descedant != null)
                {
                    if (Descedant.Equals(PrimitiveType.Char))
                        return PrimitiveType.Char;
                    
                    if (Descedant is PrimitiveType primitive && primitive.IsNumeric)
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