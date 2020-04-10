using System;
using System.Collections.Generic;
using System.Linq;

namespace nfun.Ti4
{
    public static class SolvingFunctions 
    {
        public static void MergeCycle(SolvingNode[] cycleRoute)
        {
            var main = cycleRoute.FirstOrDefault(r => r.Type == SolvingNodeType.Named) ?? cycleRoute.First();
            foreach (var current in cycleRoute)
            {
                if (current == main)
                    continue;

                if (current.NodeState is RefTo refState)
                {
                    if (!cycleRoute.Contains(refState.Node))
                        throw new NotImplementedException();
                }
                else
                {
                    //merge main and current
                    main.Ancestors.AddRange(current.Ancestors);
                    if (main.NodeState is ConcreteType concrete)
                    {
                        if (current.NodeState is ConcreteType concreteB)
                            if (!concreteB.Equals(concrete)) throw new InvalidOperationException();
                            else if (current.NodeState is SolvingConstrains constrainsB)
                            {
                                if (!constrainsB.Fits(concrete))
                                    throw new InvalidOperationException();
                                main.NodeState = concrete;
                            }
                            else throw new NotImplementedException();
                    }
                    else if (main.NodeState is SolvingConstrains constrainsA)
                    {
                        if (current.NodeState is ConcreteType concreteB)
                        {
                            if (!constrainsA.Fits(concreteB)) throw new InvalidOperationException();
                            main.NodeState = concreteB;
                        }
                        else if (current.NodeState is SolvingConstrains constrainsB)
                            main.NodeState = constrainsB.MergeOrNull(constrainsA) ?? throw new InvalidOperationException();
                        else throw new NotImplementedException();
                    }
                    else throw new NotImplementedException();
                }
                current.NodeState = new RefTo(main);
            }

            var newAncestors = cycleRoute
                .SelectMany(r => r.Ancestors)
                .Where(r => !cycleRoute.Contains(r))
                .Distinct()
                .ToList();

            main.Ancestors.Clear();
            main.Ancestors.AddRange(newAncestors);
        }

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
        private static object SetDownwardsLimits(SolvingNode descendant, SolvingNode ancestor)
        {
            #region todo проверить случаи ссылок
            if (descendant == ancestor)
                return descendant.NodeState;

            if (descendant.NodeState is RefTo referenceDesc)
            {
                referenceDesc.Node.NodeState = SetDownwardsLimits(descendant, referenceDesc.Node);
                return referenceDesc;
            }
            if (ancestor.NodeState is RefTo referenceAnc)
            {
                return SetDownwardsLimits(referenceAnc.Node, descendant);
            }
            #endregion

            ConcreteType upType = null;
            if (ancestor.NodeState is ConcreteType concreteAnc)
                upType = concreteAnc;
            else if (ancestor.NodeState is SolvingConstrains constrainsAnc)
            {

                if (constrainsAnc.Ancestor != null)
                    upType = constrainsAnc.Ancestor;
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
                constrainsDesc.AddAncestor(upType);
                return descendant.NodeState;
            }
            throw new InvalidOperationException();

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
                        var result = constrainsAnc.GetCopy();
                        result.AddDescedant(concreteDesc);
                        return result.GetOptimizedOrThrow();
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
                        if (constrainsDesc.HasDescendant && constrainsDesc.Descedant?.CanBeImplicitlyConvertedTo(concreteAnc)!=true)
                            throw new InvalidOperationException();
                        return ancestor.NodeState;
                    }
                    case SolvingConstrains constrainsAnc:
                    {
                        var result = constrainsAnc.GetCopy();
                        result.AddDescedant(constrainsDesc.Descedant);
                        return result.GetOptimizedOrThrow();
                    }
                    default:
                        throw new NotSupportedException();
                }
            }
            throw new NotSupportedException();
        }

        public static void Destruction(SolvingNode[] toposorteNodes)
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
                if (node.NodeState is RefTo)
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
                        referencedNode.NodeState = constrains.MergeOrNull(refConstrains)??throw new InvalidOperationException();
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