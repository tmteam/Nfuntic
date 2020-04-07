using System;
using System.Collections.Generic;
using System.Linq;

namespace nfun.Ti4
{
    public class SolvingConstrains
    {
        public SolvingConstrains(ConcreteType desc = null, ConcreteType anc = null)
        {
            if(desc!=null)
                DescedantTypes.Add(desc);
            if(anc!=null)
                AncestorTypes.Add(anc);
        }

        public SolvingConstrains GetCopy()
        {
            var result = new SolvingConstrains()
            {
                IsComparable = this.IsComparable,
                PreferedType = this.PreferedType
            };
            result.AncestorTypes.AddRange(AncestorTypes);
            result.DescedantTypes.AddRange(DescedantTypes);
            return result;
        }
        public bool Fits(ConcreteType concrete)
        {
            if (AncestorTypes.Any())
            {
                var anc = AncestorTypes.GetCommonDescendantOrNull();
                if (anc == null)
                    return false;
                if (!concrete.CanBeImplicitlyConvertedTo(anc))
                    return false;
            }

            if (DescedantTypes.Any())
            {
                var desc = DescedantTypes.GetCommonAncestor();
                if (!desc.CanBeImplicitlyConvertedTo(concrete))
                    return false;
            }

            if (IsComparable && !concrete.IsComparable)
                return false;
            return true;
        }

        public ConcreteType CommonAncestor 
            => AncestorTypes.Any() ? AncestorTypes.GetCommonDescendantOrNull() : null;

        public ConcreteType CommonDescedant 
            => DescedantTypes.Any() ? DescedantTypes.GetCommonAncestor() : null;

        public List<ConcreteType> AncestorTypes { get; } = new List<ConcreteType>();
        public List<ConcreteType> DescedantTypes { get; } = new List<ConcreteType>();
        public ConcreteType PreferedType { get; set; }
        public bool IsComparable { get; set; }

        public object MergeOrNull(SolvingConstrains constrains)
        {
            var result = new SolvingConstrains()
            {
                IsComparable = this.IsComparable || constrains.IsComparable
            };
            if (DescedantTypes.Any() || constrains.DescedantTypes.Any())
            {
                var descendantType = DescedantTypes.Union(constrains.DescedantTypes)
                    .GetCommonAncestor();
                result.DescedantTypes.Add(descendantType);
            }

            if (AncestorTypes.Any() || constrains.AncestorTypes.Any())
            {
                var ancestorType = AncestorTypes.Union(constrains.AncestorTypes)
                    .GetCommonDescendantOrNull();
                if (ancestorType == null)
                    return null;
                result.AncestorTypes.Add(ancestorType);
            }

            if (result.AncestorTypes.Any() && result.DescedantTypes.Any())
            {
                var anc = result.AncestorTypes[0];
                var des = result.DescedantTypes[0];
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
        public void Validate()
        {
            if(!AncestorTypes.Any())
                return;
            if(!DescedantTypes.Any())
                return;

            var des = CommonDescedant?? throw new InvalidOperationException();
            
            if(!des.CanBeImplicitlyConvertedTo(CommonAncestor))
                throw new InvalidOperationException();
        }

        public override string ToString() => $"[{CommonDescedant}..{CommonAncestor}]";

        public object GetOptimizedOrThrow()
        {
            if (IsComparable)
            {
                if (CommonDescedant != null)
                {
                    if (CommonDescedant.Equals(ConcreteType.Char))
                        return ConcreteType.Char;
                    else if (CommonDescedant.IsNumeric)
                        AncestorTypes.Add(ConcreteType.Real);
                    else
                        throw new InvalidOperationException("Types cannot be compared");
                }
            }

            if (AncestorTypes.Count > 1)
            {
                var commonAncestor = CommonAncestor;
                AncestorTypes.Clear();
                AncestorTypes.Add(commonAncestor);
            }

            if (DescedantTypes.Count > 1)
            {
                var commonDesc = CommonDescedant;
                DescedantTypes.Clear();
                DescedantTypes.Add(commonDesc);
            }

            if (CommonAncestor != null && CommonDescedant != null)
            {
                if(CommonAncestor.Equals(CommonDescedant))
                    return CommonAncestor;
                if (!CommonDescedant.CanBeImplicitlyConvertedTo(CommonAncestor))
                    throw new InvalidOperationException();
            }

            if (DescedantTypes.Any() && CommonDescedant.Equals(ConcreteType.Any))
                return ConcreteType.Any;

            return this;
        }
    }
}