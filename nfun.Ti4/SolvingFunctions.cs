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
            if(limits.IsComparable && !concrete.IsComparable)
                throw new InvalidOperationException();

            return concrete;
        }

        public static object Merge(SolvingConstrains a, SolvingConstrains b)
        {
            var result = new SolvingConstrains();
            ConcreteType ancestor = null;

            if (a.AncestorTypes.Any() || b.AncestorTypes.Any())
            {
                ancestor = a.AncestorTypes.Union(b.AncestorTypes).GetCommonDescendantOrNull();
                if (ancestor == null)
                    throw new InvalidOperationException();

                result.AncestorTypes.Add(ancestor);
            }

            ConcreteType descendant = null;

            if (a.DescedantTypes.Any() || b.DescedantTypes.Any())
            {
                descendant = a.DescedantTypes.Union(b.DescedantTypes).GetCommonAncestor();
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

            result.IsComparable = a.IsComparable || b.IsComparable;
            result.Validate();
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
                    node.NodeState = SetDownwardsLimits(node, nodeAncestor);
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
                        var result = new SolvingConstrains { 
                            PreferedType = constrainsAnc.PreferedType, 
                            IsComparable = constrainsAnc.IsComparable
                        };
                        
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
                        var result = new SolvingConstrains
                        {
                            PreferedType = constrainsAnc.PreferedType,
                            IsComparable = constrainsAnc.IsComparable,
                        };
                        result.AncestorTypes.AddRange(constrainsAnc.AncestorTypes);
                        if(constrainsAnc.DescedantTypes.Any() || constrainsDesc.DescedantTypes.Any())
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

        private static object SetDownwardsLimits(SolvingNode descendant, SolvingNode ancestor)
        {
            if (descendant.NodeState is RefTo referenceDesc)
            {
                referenceDesc.Node.NodeState = SetDownwardsLimits(descendant, referenceDesc.Node);
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

        public static void Destruction(SolvingNode[] toposorteNodes)
        {
            for (int i = toposorteNodes.Length - 1; i >= 0; i--)
            {
                var node = toposorteNodes[i];
                foreach (var ancestor in node.Ancestors.ToArray())
                {
                    TryMergeDestructive2(ancestor, node);
                }
            }
        }

        public static bool TryMergeDestructive2(SolvingNode ancestorNode, SolvingNode descendantNode)
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
            }
            else if (nonRefAncestor.NodeState is SolvingConstrains constrainsAnc)
            {
                if (nonRefDescendant.NodeState is ConcreteType concreteDesc)
                {
                    if (constrainsAnc.Fits(concreteDesc))
                    {
                        Console.WriteLine($"    {nonRefAncestor} + {nonRefDescendant} = {concreteDesc}");
                        nonRefAncestor.NodeState = concreteDesc;
                        return true;
                    }
                }
                else if (nonRefDescendant.NodeState is SolvingConstrains constrainsDesc)
                {
                    var result = constrainsAnc.MergeOrNull(constrainsDesc);
                    if (result == null)
                        return false;

                    Console.WriteLine(
                        $"    {nonRefAncestor} + {nonRefDescendant} = {result}.   {nonRefDescendant.Name}={nonRefAncestor.Name}");
                    if (result is ConcreteType)
                    {
                        nonRefAncestor.NodeState = nonRefDescendant.NodeState = descendantNode.NodeState = result;
                        return true;
                    }

                    
                    if (nonRefAncestor.Type == SolvingNodeType.TypeVariable || nonRefDescendant.Type!= SolvingNodeType.TypeVariable)
                    {
                        nonRefAncestor.NodeState = result;
                        nonRefDescendant.NodeState = descendantNode.NodeState = new RefTo(nonRefAncestor);
                    }
                    else
                    {
                        nonRefDescendant.NodeState = result;
                        nonRefAncestor.NodeState = ancestorNode.NodeState = new RefTo(nonRefDescendant);
                    }
                    nonRefDescendant.Ancestors.Remove(nonRefAncestor);
                    descendantNode.Ancestors.Remove(nonRefAncestor);

                    return true;
                }
            }
            return false;
        }

    
        public static FinalizationResults FinalizeUp(SolvingNode[] toposortedNodes)
        {
            var typeVariables = new List<SolvingNode>();
            var syntaxNodes = new SolvingNode[toposortedNodes.Length];
            var namedNodes = new List<SolvingNode>();
            
            foreach (var node in toposortedNodes)
            {
                //if(node.Ancestors.Any())
                //    throw new InvalidOperationException();

                if (node.NodeState is RefTo refTo)
                {
                    var originalOne = GetNonReference(node);
                    node.NodeState = new RefTo(originalOne);

                    if (originalOne.NodeState is ConcreteType concrete)
                    {
                        node.NodeState = concrete;
                    }
                }
                
                if (node.Type== SolvingNodeType.TypeVariable)
                {
                    typeVariables.Add(node);
                }

                if (node.Type == SolvingNodeType.Named)
                    namedNodes.Add(node);
                else if (node.Type == SolvingNodeType.SyntaxNode)
                    syntaxNodes[int.Parse(node.Name)] = node;
            }

            return new FinalizationResults(typeVariables.ToArray(), namedNodes.ToArray(), syntaxNodes);
        }

        public static bool SetEqual(SolvingNode a, SolvingNode b)
        {
            var aOrigin = GetNonReference(a);
            var bOrigin = GetNonReference(b);
            if (aOrigin != a || bOrigin != b)
                return SetEqual(aOrigin, bOrigin);

            if (a.NodeState is ConcreteType aConcrete)
            {
                if (b.NodeState is ConcreteType bConcrete)
                    return aConcrete.Equals(bConcrete);
                if (b.NodeState is SolvingConstrains solvingB)
                    return SetEqual(b, solvingB, aConcrete);
                throw  new InvalidOperationException();
            }
            if (a.NodeState is SolvingConstrains solvingA)
            {
                if (b.NodeState is ConcreteType bConcrete)
                    return SetEqual(a, solvingA, bConcrete);
                if (b.NodeState is SolvingConstrains solvingB)
                {
                    a.NodeState = Merge(solvingA, solvingB);
                    b.NodeState = new RefTo(a);
                    return true;
                }
                throw new InvalidOperationException();

            }
            throw new InvalidOperationException();

        }

        private static bool SetEqual(SolvingNode b, SolvingConstrains solvingB, ConcreteType aConcrete)
        {
            if (!solvingB.Fits(aConcrete))
                return false;
            b.NodeState = aConcrete;
            return true;
        }

        public static SolvingNode GetNonReference(this SolvingNode node)
        {
            var result = node;
            if (result.NodeState is RefTo referenceA)
            {
                result = referenceA.Node;
                if (result.NodeState is RefTo)
                    return GetNonReference(result);
            }
            return result;
        }

        public static void BecomeAncestorFor(this SolvingNode ancestor, SolvingNode descendant)
        {
            descendant.Ancestors.Add(ancestor);
        }
        public static void BecomeReferenceFor(this SolvingNode referencedNode, SolvingNode otherNode)
        {
            if (otherNode.NodeState is RefTo refTo)
            {
                otherNode.NodeState = new RefTo(referencedNode);
                BecomeReferenceFor(referencedNode, refTo.Node);
                return;
            }

            if (referencedNode.NodeState is ConcreteType refConcrete)
            {
                switch (otherNode.NodeState)
                {
                    case ConcreteType c when !c.Equals(refConcrete):
                        throw new InvalidOperationException();
                    case ConcreteType _: 
                        return;
                    case SolvingConstrains constrains when constrains.Fits(refConcrete):
                        otherNode.NodeState = refConcrete;
                        return;
                    case SolvingConstrains _: 
                        throw new InvalidOperationException();
                    default:
                        throw new NotSupportedException();
                }
            }

            if (referencedNode.NodeState is SolvingConstrains refConstrains)
            {
                switch (otherNode.NodeState)
                {
                    case ConcreteType concrete when refConstrains.Fits(concrete):
                        referencedNode.NodeState = concrete;
                        return;
                    case ConcreteType _: 
                        throw  new InvalidOperationException();
                    case SolvingConstrains constrains:
                        referencedNode.NodeState = Merge(constrains, refConstrains);
                        otherNode.NodeState = new RefTo(referencedNode);
                        return;
                    default:
                        throw new NotSupportedException();
                }
            }

            throw new NotSupportedException();
        }
    }
}