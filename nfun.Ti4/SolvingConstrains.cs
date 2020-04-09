using System;
using Microsoft.Win32.SafeHandles;

namespace nfun.Ti4
{
    public class SolvingConstrains
    {
        public SolvingConstrains(ConcreteType desc = null, ConcreteType anc = null)
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
        public bool Fits(ConcreteType concrete)
        {
            if (HasAncestor)
            {
                if (!concrete.CanBeImplicitlyConvertedTo(Ancestor))
                    return false;
            }

            if (HasDescendant)
            {
                if (!Descedant.CanBeImplicitlyConvertedTo(concrete))
                    return false;
            }

            if (IsArray && !(concrete is ConcreteArrayType))
                return false;

            if (IsComparable && !concrete.IsComparable)
                return false;
            return true;
        }

        public bool IsArray => ArrayElementState != null;
        public SolvingNode ArrayElementState { get; private set; }

        public void BecomeArray(SolvingNode elementNode)
        {
            if(IsComparable)
                throw new InvalidOperationException();

            ArrayElementState = elementNode;
            if (HasAncestor)
            {
                if (Ancestor is ConcreteArrayType arrayAnc)
                    elementNode.SetAncestor(arrayAnc.ElementType);
                else
                    throw new InvalidOperationException();
                Ancestor = null;
            }

            if (HasDescendant)
            {
                if (Descedant is ConcreteArrayType arrayDes)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    throw new InvalidOperationException();
                }

                Descedant = null;
            }
        }

        public ConcreteType Ancestor { get; private set; }
        public ConcreteType Descedant { get; private set; }

        public bool HasAncestor => Ancestor!=null;
        public bool HasDescendant => Descedant!=null;

        public bool TryAddAncestor(ConcreteType type)
        {
            if (type == null)
                return true;
            if (IsArray)
            {
                if (type is ConcreteArrayType array)
                {
                    return ArrayElementState.TrySetAncestor(array.ElementType);
                }
                return false;
            }
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
        public void AddAncestor(ConcreteType type)
        {
            if(!TryAddAncestor(type))
                throw new InvalidOperationException();
        }

        public void AddDescedant(ConcreteType type)
        {
            if(type==null)
                return;
            if (IsArray)
                throw new InvalidOperationException();

            if (Descedant == null)
                Descedant = type;
            else
                Descedant = Descedant.GetLastCommonAncestor(type);
        }
        public ConcreteType PreferedType { get; set; }
        public bool IsComparable { get; set; }

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

            if (IsArray && constrains.IsArray)
            {
                this.ArrayElementState.BecomeReferenceFor(constrains.ArrayElementState);
                result.ArrayElementState = ArrayElementState;
            }
            else if (IsArray)
                result.ArrayElementState = ArrayElementState;
            else if (constrains.IsArray)
                result.ArrayElementState = constrains.ArrayElementState;
            return result;
        }
       
        public override string ToString() => $"[{Descedant}..{Ancestor}]";

        public object GetOptimizedOrThrow()
        {
            if (IsComparable)
            {
                if (Descedant != null)
                {
                    if (Descedant.Equals(ConcreteType.Char))
                        return ConcreteType.Char;
                    
                    if (Descedant.IsNumeric)
                    {
                        if(!TryAddAncestor(ConcreteType.Real))
                            throw new InvalidOperationException();
                    }
                    else
                        throw new InvalidOperationException("Types cannot be compared");
                }
            }

            if (IsArray)
            {
                if(HasAncestor || HasDescendant || IsComparable)
                    throw new InvalidOperationException();
                if (ArrayElementState.GetNonReference().NodeState is ConcreteType concrete)
                    return ConcreteType.ArrayOf(concrete);
            }
            if (HasAncestor && HasDescendant)
            {
                if(Ancestor.Equals(Descedant))
                    return Ancestor;
                if (!Descedant.CanBeImplicitlyConvertedTo(Ancestor))
                    throw new InvalidOperationException();
            }

            if (Descedant?.Equals(ConcreteType.Any)==true)
                return ConcreteType.Any;

            return this;
        }
    }
}