using System;
using System.Collections.Generic;
using System.Linq;

namespace nfun.Ti4
{
    public static class SolvingFunctions
    {
        public static object Merge(ConcreteType a, ConcreteType b)
        {
            if (a.Name != b.Name)
                throw new InvalidOperationException();
            return a;
        }

        public static object Merge(SolvingConstrains limits, ConcreteType concrete)
        {
            if (!limits.AncestorTypes.TrueForAll(concrete.CanBeImplicitlyConvertedTo))
                throw new InvalidOperationException();
            if (!limits.DescedantTypes.TrueForAll(d => d.CanBeImplicitlyConvertedTo(concrete)))
                throw new InvalidOperationException();
            return concrete;
        }

        public static object Merge(SolvingConstrains a, SolvingConstrains b)
        {
            var result = new SolvingConstrains();
            ConcreteType ancestor = null;

            if (a.AncestorTypes.Any() || b.AncestorTypes.Any())
            {
                ancestor = a.AncestorTypes.Union(b.AncestorTypes).GetCommonAncestor();
                result.AncestorTypes.Add(ancestor);
            }

            ConcreteType descendant = null;

            if (a.DescedantTypes.Any() || b.DescedantTypes.Any())
            {
                descendant = a.DescedantTypes.Union(b.DescedantTypes).GetCommonDescendantOrNull();
                if (descendant == null)
                    throw new InvalidOperationException();
                result.DescedantTypes.Add(descendant);
            }

            if (ancestor != null && descendant != null)
                if (!descendant.CanBeImplicitlyConvertedTo(ancestor))
                    throw new InvalidOperationException();
            if (a.PreferedType != null)
            {
                if (b.PreferedType == null || a.PreferedType.Name == b.PreferedType.Name)
                {
                    result.PreferedType = a.PreferedType;
                }
            }
            else
                result.PreferedType = b.PreferedType;

            return result;
        }
        public static ConcreteType GetCommonAncestor(this IEnumerable<ConcreteType> types)
            => types.Aggregate((t1, t2) => t1.GetLastCommonAncestor(t2));
        public static ConcreteType GetCommonDescendantOrNull(this IEnumerable<ConcreteType> types)
            => types.Aggregate((t1, t2) => t1?.GetFirstCommonDescendantOrNull(t2));

        public static void SetDownwardsLimits(SolvingNode[] toposortedNodes)
        {
            for (int i = toposortedNodes.Length - 1; i >= 0; i--)
            {
                var node = toposortedNodes[i];
                foreach (var nodeAncestor in node.Ancestors)
                {
                    node.NodeState = SetDownwardsLimitis(node, nodeAncestor);
                }
            }
        }

        public static void SetUpwardsLimits(SolvingNode[] toposortedNodes)
        {
            foreach (var node in toposortedNodes)
            {
                for (var index = 0; index < node.Ancestors.Count; index++)
                {
                    var ancestor = node.Ancestors[index];
                    ancestor.NodeState = SetUpwardsLimits(node, ancestor);
                }
            }
        }

        private static object SetUpwardsLimits(SolvingNode descendant, SolvingNode ancestor)
        {
            #region handle refto cases. 
            if (ancestor == descendant)
                return ancestor.NodeState;

            if (ancestor.NodeState is RefTo referenceAnc)
            {
                if (descendant.Ancestors.Contains(ancestor))
                {
                    descendant.Ancestors.Remove(ancestor);
                    if(descendant!= referenceAnc.Node)
                        descendant.Ancestors.Add(referenceAnc.Node);
                }
                referenceAnc.Node.NodeState = SetUpwardsLimits(descendant, referenceAnc.Node);
                return referenceAnc;
            }

            if (descendant.NodeState is RefTo referenceDesc)
            {
                if (descendant.Ancestors.Contains(ancestor))
                {
                    descendant.Ancestors.Remove(ancestor);
                    if(referenceDesc.Node!= ancestor)
                        referenceDesc.Node.Ancestors.Add(ancestor);
                }
                ancestor.NodeState = SetUpwardsLimits(referenceDesc.Node, ancestor);
                return ancestor.NodeState;
            }
            #endregion

            if (descendant.NodeState is ConcreteType concreteDesc)
            {
                switch (ancestor.NodeState)
                {
                    case ConcreteType concreteAnc:
                    {
                        if (!concreteDesc.CanBeImplicitlyConvertedTo(concreteAnc))
                            throw new InvalidOperationException();
                        return ancestor.NodeState;
                    }
                    case SolvingConstrains constrainsAnc:
                    {
                        var result = new SolvingConstrains { PreferedType = constrainsAnc.PreferedType };
                        result.AncestorTypes.AddRange(
                            constrainsAnc.AncestorTypes);
                        result.DescedantTypes.Add(
                            constrainsAnc.DescedantTypes.Append(concreteDesc).GetCommonAncestor());
                        result.Validate();
                        return result;
                    }
                    default:
                        throw new NotSupportedException();
                }
            }

            if (descendant.NodeState is SolvingConstrains constrainsDesc)
            {
                switch (ancestor.NodeState)
                {
                    case ConcreteType concreteAnc:
                    {
                        if (constrainsDesc.DescedantTypes.Any()
                            && constrainsDesc.DescedantTypes.GetCommonAncestor()?.CanBeImplicitlyConvertedTo(concreteAnc) != true)
                            throw new InvalidOperationException();
                        return ancestor.NodeState;
                    }
                    case SolvingConstrains constrainsAnc:
                    {
                        var result = new SolvingConstrains { PreferedType = constrainsAnc.PreferedType };
                        result.AncestorTypes.AddRange(constrainsAnc.AncestorTypes);
                        result.DescedantTypes.Add(constrainsAnc.DescedantTypes.Concat(constrainsDesc.DescedantTypes).GetCommonAncestor());
                        result.Validate();
                        return result;
                    }
                    default:
                        throw new NotSupportedException();
                }
            }
            throw new NotSupportedException();
        }

        private static object SetDownwardsLimitis(SolvingNode descendant, SolvingNode ancestor)
        {
            if (descendant.NodeState is RefTo referenceDesc)
            {
                referenceDesc.Node.NodeState = SetDownwardsLimitis(descendant, referenceDesc.Node);
                return referenceDesc;
            }

            ConcreteType upType = null;
            if (ancestor.NodeState is ConcreteType concreteAnc)
                upType = concreteAnc;
            else if (ancestor.NodeState is SolvingConstrains constrainsAnc)
            {
                if (constrainsAnc.AncestorTypes.Any())
                    upType = constrainsAnc.AncestorTypes.GetCommonDescendantOrNull();
                else
                    return descendant.NodeState;
            }
            if (upType == null)
                throw new InvalidOperationException();

            if (descendant.NodeState is ConcreteType concreteDesc)
            {
                if (concreteDesc.CanBeImplicitlyConvertedTo(upType))
                    return descendant.NodeState;
                throw new InvalidOperationException();
            }
            if (descendant.NodeState is SolvingConstrains constrainsDesc)
            {
                var newUpLimit = constrainsDesc.AncestorTypes.Append(upType).GetCommonDescendantOrNull();
                if (newUpLimit == null)
                    throw new InvalidOperationException();
                constrainsDesc.AncestorTypes.Clear();
                constrainsDesc.AncestorTypes.Add(newUpLimit);
                return descendant.NodeState;
            }
            throw new InvalidOperationException();

        }

        public static void DestructiveMergeAll(SolvingNode[] toposorteNodes)
        {
            for (int i = toposorteNodes.Length - 1; i >= 0; i--)
            {
                var node = toposorteNodes[i];
                foreach (var ancestor in node.Ancestors.ToArray())
                {
                    TryMergeDestructive(ancestor, node);
                }
            }
        }
        public static bool TryMergeDestructive(SolvingNode ancestorNode, SolvingNode descendantNode)
        {
            Console.WriteLine($"-dm: {ancestorNode} -> {descendantNode}");
            var nonRefAncestor = GetNonReference(ancestorNode);
            var nonRefDescendant = GetNonReference(descendantNode);
            if (nonRefDescendant == nonRefAncestor)
            {
                Console.WriteLine($"Same deref. Skip");
                return false;
            }
        
            if (nonRefAncestor.NodeState is ConcreteType concreteAnc)
            {
                if (nonRefDescendant.NodeState is SolvingConstrains constrainsDesc)
                {
                    if (constrainsDesc.Fits(concreteAnc))
                    {
                        Console.WriteLine($"    {nonRefAncestor} + {nonRefDescendant} = {concreteAnc}");
                        nonRefDescendant.NodeState = nonRefAncestor.NodeState;
                        return true;
                    }
                }
                return false;
            }

            if (nonRefAncestor.NodeState is SolvingConstrains constrainsAnc)
            {
                if (nonRefDescendant.NodeState is SolvingConstrains constrainsDesc)
                {
                    var result = new SolvingConstrains();
                    if (constrainsAnc.DescedantTypes.Any() || constrainsDesc.DescedantTypes.Any())
                    {
                        var descendantType = constrainsAnc.DescedantTypes.Union(constrainsDesc.DescedantTypes).GetCommonAncestor();
                        if (descendantType == null)
                            throw new InvalidOperationException();
                        result.DescedantTypes.Add(descendantType);
                    }
                    if (constrainsAnc.AncestorTypes.Any() || constrainsDesc.AncestorTypes.Any())
                    {
                        var ancestorType = constrainsAnc.AncestorTypes.Union(constrainsDesc.AncestorTypes).GetCommonDescendantOrNull();
                        result.AncestorTypes.Add(ancestorType);
                    }
                    Console.WriteLine($"    {nonRefAncestor} + {nonRefDescendant} = {result}.   {nonRefDescendant.Name}={nonRefAncestor.Name}");
                    
                    nonRefAncestor.NodeState = result;
                    nonRefDescendant.NodeState = descendantNode.NodeState= new RefTo(nonRefAncestor);
                    nonRefDescendant.Ancestors.Remove(nonRefAncestor);
                    descendantNode.Ancestors.Remove(nonRefAncestor);

                    return true;
                }
                else if (nonRefDescendant.NodeState is ConcreteType concreteDesc)
                {
                    if (constrainsAnc.Fits(concreteDesc))
                    {
                        Console.WriteLine($"    {nonRefAncestor} + {nonRefDescendant} = {concreteDesc}");

                        nonRefAncestor.NodeState = concreteDesc;
                        return true;
                    }

                    return false;
                }
            }
            throw new InvalidOperationException();
        }

        private static SolvingNode GetNonReference(SolvingNode node)
        {
            var result = node;
            if (result.NodeState is RefTo referenceA)
            {
                result = referenceA.Node;
                if (result.NodeState is RefTo)
                    throw new InvalidOperationException();
            }
            return result;
        }
    }
}