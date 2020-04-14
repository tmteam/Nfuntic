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

                if (current.State is RefTo refState)
                {
                    if (!cycleRoute.Contains(refState.Node))
                        throw new NotImplementedException();
                }
                else
                {
                    //merge main and current
                    main.Ancestors.AddRange(current.Ancestors);
                    if (main.State is PrimitiveType concrete)
                    {
                        if (current.State is PrimitiveType concreteB)
                            if (!concreteB.Equals(concrete)) throw new InvalidOperationException();
                            else if (current.State is SolvingConstrains constrainsB)
                            {
                                if (!constrainsB.Fits(concrete))
                                    throw new InvalidOperationException();
                                main.State = concrete;
                            }
                            else throw new NotImplementedException();
                    }
                    else if (main.State is SolvingConstrains constrainsA)
                    {
                        if (current.State is PrimitiveType concreteB)
                        {
                            if (!constrainsA.Fits(concreteB)) throw new InvalidOperationException();
                            main.State = concreteB;
                        }
                        else if (current.State is SolvingConstrains constrainsB)
                            main.State = constrainsB.MergeOrNull(constrainsA) ?? throw new InvalidOperationException();
                        else throw new NotImplementedException();
                    }
                    else throw new NotImplementedException();
                }
                if(!current.IsSolved)
                    current.State = new RefTo(main);
            }

            var newAncestors = cycleRoute
                .SelectMany(r => r.Ancestors)
                .Where(r => !cycleRoute.Contains(r))
                .Distinct()
                .ToList();

            main.Ancestors.Clear();
            main.Ancestors.AddRange(newAncestors);
        }

        #region Upward 

        public static void SetUpwardsLimits(SolvingNode[] toposortedNodes)
        {
            void HandleUpwardLimits(SolvingNode node)
            {
                for (var index = 0; index < node.Ancestors.Count; index++)
                {
                    var ancestor = node.Ancestors[index];
                    ancestor.State = SetUpwardsLimits(node, ancestor);
                }
                if (node.State is ArrayOf array) 
                    HandleUpwardLimits(array.ElementNode);
            }

            foreach (var node in toposortedNodes)
                HandleUpwardLimits(node);
        }

        private static object SetUpwardsLimits(SolvingNode descendant, SolvingNode ancestor)
        {
            #region handle refto cases. 
            if (ancestor == descendant)
                return ancestor.State;

            if (ancestor.State is RefTo referenceAnc)
            {
                if (descendant.Ancestors.Contains(ancestor))
                {
                    descendant.Ancestors.Remove(ancestor);
                    if (descendant != referenceAnc.Node)
                        descendant.Ancestors.Add(referenceAnc.Node);
                }
                referenceAnc.Node.State = SetUpwardsLimits(descendant, referenceAnc.Node);
                return referenceAnc;
            }

            if (descendant.State is RefTo referenceDesc)
            {
                if (descendant.Ancestors.Contains(ancestor))
                {
                    descendant.Ancestors.Remove(ancestor);
                    if (referenceDesc.Node != ancestor)
                        referenceDesc.Node.Ancestors.Add(ancestor);
                }
                ancestor.State = SetUpwardsLimits(referenceDesc.Node, ancestor);
                return ancestor.State;
            }
            #endregion

            if (descendant.State is IType typeDesc)
            {
                switch (ancestor.State)
                {
                    case PrimitiveType concreteAnc:
                    {
                        if (!typeDesc.CanBeImplicitlyConvertedTo(concreteAnc))
                            throw new InvalidOperationException();
                        return ancestor.State;
                    }
                    case SolvingConstrains constrainsAnc:
                    {
                        var result = constrainsAnc.GetCopy();
                        result.AddDescedant(typeDesc);
                        return result.GetOptimizedOrThrow();
                    }
                    case ArrayOf arrayAnc:
                    {
                        if (!(typeDesc is ArrayOf arrayDesc))
                            throw new NotSupportedException();
                        
                        descendant.Ancestors.Remove(ancestor);
                        arrayDesc.ElementNode.Ancestors.Add(arrayAnc.ElementNode);
                        return ancestor.State;
                    }
                    default:
                        throw new NotSupportedException();
                }
            }
            /*
            if (descendant.State is PrimitiveType concreteDesc)
            {
                switch (ancestor.State)
                {
                    case PrimitiveType concreteAnc:
                        {
                            if (!concreteDesc.CanBeImplicitlyConvertedTo(concreteAnc))
                                throw new InvalidOperationException();
                            return ancestor.State;
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
            }*/

            if (descendant.State is SolvingConstrains constrainsDesc)
            {
                switch (ancestor.State)
                {
                    case PrimitiveType concreteAnc:
                        {
                            if (constrainsDesc.HasDescendant && constrainsDesc.Descedant?.CanBeImplicitlyConvertedTo(concreteAnc) != true)
                                throw new InvalidOperationException();
                            return ancestor.State;
                        }
                    case SolvingConstrains constrainsAnc:
                        {
                            var result = constrainsAnc.GetCopy();
                            result.AddDescedant(constrainsDesc.Descedant);
                            return result.GetOptimizedOrThrow();
                        }
                    case ArrayOf arrayAnc:
                    {
                        var result = TransformToArrayOrNull(descendant.Name, constrainsDesc, arrayAnc);
                        if(result==null)
                            throw new InvalidOperationException();
                        descendant.State = result;
                        return ancestor.State;
                    }
                    default:
                        throw new NotSupportedException($"Ancestor type {ancestor.State.GetType().Name} is not supported");
                }
            }
            /*
            if (descendant.State is ArrayOf desArrayOf)
            {
                switch (ancestor.State)
                {
                    case ArrayOf ancArrayOf:
                        {
                            var ancE = ancArrayOf.ElementNode;
                            var desE = desArrayOf.ElementNode;
                            ancE.State = SetUpwardsLimits(desE, ancE);
                            return ancestor.State;
                        }
                    case SolvingConstrains _: return ancestor.State;
                    default:
                        throw new NotSupportedException($"Ancestor type {ancestor.State.GetType().Name} is not supported");
                }
            }*/

            throw new NotSupportedException($"Descendant type {descendant.State.GetType().Name} is not supported");
        }

        /// <summary>
        /// Превращает неопределенное ограничение в ограничение с массивом
        /// </summary>
        /// <param name="descNodeName"></param>
        /// <param name="descendant"></param>
        /// <param name="ancestor"></param>
        /// <returns></returns>
        public static object TransformToArrayOrNull(string descNodeName, SolvingConstrains descendant, ArrayOf ancestor)
        {
            if (!descendant.NoConstrains)
                return null;
            var constrains = new SolvingConstrains();
            var node = new SolvingNode(descNodeName+"'", constrains, SolvingNodeType.TypeVariable);
            node.Ancestors.Add(ancestor.ElementNode);
            return new ArrayOf(node);
        }

        #endregion
        public static void SetDownwardsLimits(SolvingNode[] toposortedNodes)
        {
            for (int i = toposortedNodes.Length - 1; i >= 0; i--)
            {
                var descendant = toposortedNodes[i];
                foreach (var ancestor in descendant.Ancestors)
                    descendant.State = SetDownwardsLimits(descendant, ancestor);
            }
        }
        private static object SetDownwardsLimits(SolvingNode descendant, SolvingNode ancestor)
        {
            #region todo проверить случаи ссылок
            if (descendant == ancestor)
                return descendant.State;

            if (descendant.State is RefTo referenceDesc)
            {
                referenceDesc.Node.State = SetDownwardsLimits(descendant, referenceDesc.Node);
                return referenceDesc;
            }
            if (ancestor.State is RefTo referenceAnc)
            {
                return SetDownwardsLimits(referenceAnc.Node, descendant);
            }
            #endregion

            if (ancestor.State is ArrayOf ancArray)
            {
                if (descendant.State is SolvingConstrains constr && constr.NoConstrains)
                {
                    descendant.State = new ArrayOf(
                        new SolvingNode(descendant.Name + "`", new SolvingConstrains(), SolvingNodeType.TypeVariable));
                }

                if (descendant.State is ArrayOf desArray)
                {
                    desArray.ElementNode.State = SetDownwardsLimits(desArray.ElementNode, ancArray.ElementNode);
                    return descendant.State;
                }
                throw new InvalidOperationException();
            }

            PrimitiveType upType = null;
            if (ancestor.State is PrimitiveType concreteAnc) upType = concreteAnc;
            else if (ancestor.State is SolvingConstrains constrainsAnc)
            {
                if (constrainsAnc.HasAncestor) upType = constrainsAnc.Ancestor;
                else return descendant.State;
            }
            else if (ancestor.State is ArrayOf) return descendant.State;

            if (upType == null)
                throw new InvalidOperationException();

            if (descendant.State is PrimitiveType concreteDesc)
            {
                if (concreteDesc.CanBeImplicitlyConvertedTo(upType))
                    return descendant.State;
                throw new InvalidOperationException();
            }
            if (descendant.State is SolvingConstrains constrainsDesc)
            {
                constrainsDesc.AddAncestor(upType);
                return descendant.State;
            }

            if (descendant.State is ArrayOf array)
            {
                if(!array.TrySetAncestor(upType))
                    throw new InvalidOperationException();
                return array;
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

            if (nonRefAncestor.State is PrimitiveType concreteAnc)
            {
                if (nonRefDescendant.State is SolvingConstrains constrainsDesc)
                {
                    if (constrainsDesc.Fits(concreteAnc))
                    {
                        Console.WriteLine($"    {nonRefAncestor} + {nonRefDescendant} = {concreteAnc}");
                        nonRefDescendant.State = nonRefAncestor.State;
                        return true;
                    }
                }
            }
            else if (nonRefAncestor.State is SolvingConstrains constrainsAnc)
            {
                if (nonRefDescendant.State is PrimitiveType concreteDesc)
                {
                    if (constrainsAnc.Fits(concreteDesc))
                    {
                        Console.WriteLine($"    {nonRefAncestor} + {nonRefDescendant} = {concreteDesc}");
                        nonRefAncestor.State = concreteDesc;
                        return true;
                    }
                }
                else if (nonRefDescendant.State is SolvingConstrains constrainsDesc)
                {
                    var result = constrainsAnc.MergeOrNull(constrainsDesc);
                    if (result == null)
                        return false;

                    Console.WriteLine(
                        $"    {nonRefAncestor} + {nonRefDescendant} = {result}.   {nonRefDescendant.Name}={nonRefAncestor.Name}");
                    if (result is PrimitiveType)
                    {
                        nonRefAncestor.State = nonRefDescendant.State = descendantNode.State = result;
                        return true;
                    }

                    
                    if (nonRefAncestor.Type == SolvingNodeType.TypeVariable || nonRefDescendant.Type!= SolvingNodeType.TypeVariable)
                    {
                        nonRefAncestor.State = result;
                        nonRefDescendant.State = descendantNode.State = new RefTo(nonRefAncestor);
                    }
                    else
                    {
                        nonRefDescendant.State = result;
                        nonRefAncestor.State = ancestorNode.State = new RefTo(nonRefDescendant);
                    }
                    nonRefDescendant.Ancestors.Remove(nonRefAncestor);
                    descendantNode.Ancestors.Remove(nonRefAncestor);

                    return true;
                }
                else if (nonRefDescendant.State is ArrayOf arrayDes)
                {
                    if(constrainsAnc.HasDescendant && constrainsAnc.Descedant!= PrimitiveType.Any)
                        throw new InvalidOperationException();

                    if (constrainsAnc.HasAncestor && !constrainsAnc.Ancestor.Equals(PrimitiveType.Any))
                        throw new InvalidOperationException();
                    
                    ancestorNode.State = new RefTo(nonRefDescendant);
                }
            }
            else if (nonRefAncestor.State is ArrayOf arrayAnc)
            {
                if (nonRefDescendant.State is SolvingConstrains constrainsDesc && constrainsDesc.NoConstrains)
                {
                    nonRefDescendant.State = new RefTo(nonRefAncestor);
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
                if (node.State is RefTo)
                {
                    var originalOne = GetNonReference(node);
                    node.State = new RefTo(originalOne);

                    if (originalOne.State is PrimitiveType concrete)
                    {
                        node.State = concrete;
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
            if (result.State is RefTo referenceA)
            {
                result = referenceA.Node;
                if (result.State is RefTo)
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
            if (otherNode.State is RefTo refTo)
            {
                otherNode.State = new RefTo(referencedNode);
                BecomeReferenceFor(referencedNode, refTo.Node);
                return;
            }

            if (referencedNode.State is PrimitiveType refConcrete)
            {
                switch (otherNode.State)
                {
                    case PrimitiveType c when !c.Equals(refConcrete):
                        throw new InvalidOperationException();
                    case PrimitiveType _: 
                        return;
                    case SolvingConstrains constrains when constrains.Fits(refConcrete):
                        otherNode.State = refConcrete;
                        return;
                    case SolvingConstrains _: 
                        throw new InvalidOperationException();
                    default:
                        throw new NotSupportedException();
                }
            }

            if (referencedNode.State is SolvingConstrains refConstrains)
            {
                switch (otherNode.State)
                {
                    case PrimitiveType concrete when refConstrains.Fits(concrete):
                        referencedNode.State = concrete;
                        return;
                    case PrimitiveType _: 
                        throw  new InvalidOperationException();
                    case SolvingConstrains constrains:
                        referencedNode.State = constrains.MergeOrNull(refConstrains)??throw new InvalidOperationException();
                        otherNode.State = new RefTo(referencedNode);
                        return;
                    default:
                        throw new NotSupportedException();
                }
            }

            throw new NotSupportedException();
        }
    }
}