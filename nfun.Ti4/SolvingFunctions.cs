using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace nfun.Ti4
{
    public static class SolvingFunctions 
    {
        #region Merges

        public static object GetMergedState(object stateA, object stateB)
        {
            if (stateB is SolvingConstrains c && c.NoConstrains)
                return stateA;

            if (stateA is IType typeA && typeA.IsSolved)
            {
                if (stateB is IType typeB && typeB.IsSolved)
                {
                    if (!typeB.Equals(typeA)) 
                        throw new InvalidOperationException();
                    return typeA;
                }
                if (stateB is SolvingConstrains constrainsB)
                {
                    if (!constrainsB.Fits(typeA))
                        throw new InvalidOperationException();
                    return typeA;
                }
            }
            if (stateA is ArrayOf arrayA)
            {
                if (stateB is ArrayOf arrayB)
                {
                    Merge(arrayA.ElementNode, arrayB.ElementNode);
                    return arrayA;
                }
            }
            if (stateA is SolvingConstrains constrainsA)
            {
                if (stateB is SolvingConstrains constrainsB)
                    return constrainsB.MergeOrNull(constrainsA) ?? throw new InvalidOperationException();
                return GetMergedState(stateB, stateA);
            }
            if (stateA is RefTo refA)
            {
                var state = GetMergedState(refA.Node.State, stateB);
                refA.Node.State = state;
                return stateA;
            }
            if (stateB is RefTo)
            {
                return GetMergedState(stateB, stateA);
            }
            throw new InvalidOperationException();
        }
        public static void Merge(SolvingNode main, SolvingNode secondary)
        {
            var res = GetMergedState(main.State, secondary.State);
            main.State = res;
            if (res is IType t && t.IsSolved)
            {
                secondary.State = res;
                return;
            }

            main.Ancestors.AddRange(secondary.Ancestors);
            secondary.Ancestors.Clear();
            secondary.State = new RefTo(main);
        }

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
                    main.State = GetMergedState(main.State, current.State);
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
    #endregion

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

            foreach (var node in toposortedNodes.Where(n=>!n.MemberOf.Any()))
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
                        
                        result.ElementNode.Ancestors.Add(arrayAnc.ElementNode);
                        descendant.State = result;
                        descendant.Ancestors.Remove(ancestor);
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
        #endregion

        #region Downward

        public static void SetDownwardsLimits(SolvingNode[] toposortedNodes)
        {
            void Downwards(SolvingNode descendant)
            {
                if (descendant.State is ArrayOf array)
                    Downwards(array.ElementNode);
                foreach (var ancestor in descendant.Ancestors)
                    descendant.State = SetDownwardsLimits(descendant, ancestor);
            }

            for (int i = toposortedNodes.Length - 1; i >= 0; i--)
            {

                var descendant = toposortedNodes[i];
                if(descendant.MemberOf.Any())
                    continue;
                
                Downwards(descendant);
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
                if (descendant.State is SolvingConstrains constr)
                {
                    var result = TransformToArrayOrNull(descendant.Name, constr, ancArray);
                    if(result==null)
                        throw new InvalidOperationException();
                    descendant.State = result;
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

            if (descendant.State is IType concreteDesc)
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

            throw new InvalidOperationException();

        }
        #endregion

        #region Destruction

        public static void Destruction(SolvingNode[] toposorteNodes)
        {
            void Destruction(SolvingNode node)
            {
                if (node.State is ArrayOf arrayDesc)
                {
                    Destruction(arrayDesc.ElementNode);
                    if (arrayDesc.Element is RefTo refTo)
                       node.State = new ArrayOf(refTo.Node);
                }

                foreach (var ancestor in node.Ancestors.ToArray())
                {
                    TryMergeDestructive(node, ancestor);
                }
            }
            for (int i = toposorteNodes.Length - 1; i >= 0; i--)
            {
                var descendant = toposorteNodes[i];
                if (descendant.MemberOf.Any())
                    continue;
                Destruction(descendant);
            }
        }

        public static void TryMergeDestructive(SolvingNode descendantNode, SolvingNode ancestorNode)
        {
            Console.WriteLine($"-dm: {ancestorNode} -> {descendantNode}");
            var nonRefAncestor = GetNonReference(ancestorNode);
            var nonRefDescendant = GetNonReference(descendantNode);
            if (nonRefDescendant == nonRefAncestor)
            {
                Console.WriteLine($"Same deref. Skip");
                return;
            }

            var originAnc = nonRefAncestor.ToString();
            var originDes = nonRefDescendant.ToString();


            switch (nonRefAncestor.State)
            {
                case PrimitiveType concreteAnc:
                {
                    if (nonRefDescendant.State is SolvingConstrains c && c.Fits(concreteAnc))
                    {
                        Console.Write("p+c");
                        nonRefDescendant.State = concreteAnc;
                    }
                    break;
                }

                case ArrayOf arrayAnc:
                {

                    if (nonRefDescendant.State is SolvingConstrains constrainsDesc && constrainsDesc.Fits(arrayAnc))
                    {
                        Console.Write("a+c");
                        nonRefDescendant.State = new RefTo(nonRefAncestor);
                    }

                    break;
                }

                case SolvingConstrains constrainsAnc when nonRefDescendant.State is PrimitiveType concreteDesc:
                {
                    if (constrainsAnc.Fits(concreteDesc))
                    {
                        Console.Write("c+p");
                        descendantNode.State = ancestorNode.State = nonRefAncestor.State = concreteDesc;
                    }

                    break;
                }

                case SolvingConstrains constrainsAnc when nonRefDescendant.State is SolvingConstrains constrainsDesc:
                {
                    Console.Write("c+c");

                    var result = constrainsAnc.MergeOrNull(constrainsDesc);
                    if (result == null)
                        return;

                    if (result is PrimitiveType)
                    {
                        nonRefAncestor.State = nonRefDescendant.State = descendantNode.State = result;
                        return;
                    }

                    if (nonRefAncestor.Type == SolvingNodeType.TypeVariable ||
                        nonRefDescendant.Type != SolvingNodeType.TypeVariable)
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
                    break;
                }

                case SolvingConstrains constrainsAnc when nonRefDescendant.State is ArrayOf arrayDes:
                {
                    Console.Write("c+a");

                    if (constrainsAnc.Fits(arrayDes))
                        nonRefAncestor.State = ancestorNode.State = new RefTo(nonRefDescendant);

                    break;
                }
            }
            
            Console.WriteLine($"    {originAnc} + {originDes} = {nonRefDescendant.State}");
        }

        #endregion

        #region Finalize

        public static FinalizationResults FinalizeUp(SolvingNode[] toposortedNodes)
        {
            var typeVariables = new HashSet<SolvingNode>();
            var syntaxNodes = new SolvingNode[toposortedNodes.Length];
            var namedNodes = new List<SolvingNode>();

            void Finalize(SolvingNode node)
            {
                if (node.State is RefTo)
                {
                    var originalOne = node.GetNonReference();
                    if (originalOne != node)
                    {
                        Console.WriteLine($"\t{node.Name}->r");
                        node.State = new RefTo(originalOne);
                    }

                    if (originalOne.State is IType)
                    {
                        node.State = originalOne.State;
                        Console.WriteLine($"\t{node.Name}->s");
                    }
                }
                else if (node.State is ArrayOf array)
                {
                    if (array.Element is RefTo reference)
                    {
                        node.State = new ArrayOf(reference.Node.GetNonReference());
                        Console.WriteLine($"\t{node.Name}->ar");
                    }
                    Finalize((node.State as ArrayOf).ElementNode);
                }
            }

            foreach (var node in toposortedNodes)
            {
                Finalize(node);

                var concreteElement = node.GetTypeLeafElement();

                if (concreteElement.Type== SolvingNodeType.TypeVariable 
                    && concreteElement.State is SolvingConstrains)
                {
                    if(!typeVariables.Contains(concreteElement))
                        typeVariables.Add(concreteElement);
                }


                if (node.Type == SolvingNodeType.Named)
                    namedNodes.Add(node);
                else if (node.Type == SolvingNodeType.SyntaxNode)
                    syntaxNodes[int.Parse(node.Name)] = node;
            }

            return new FinalizationResults(typeVariables.ToArray(), namedNodes.ToArray(), syntaxNodes);
        }
        #endregion

        public static SolvingNode GetTypeLeafElement(this SolvingNode node)
        {
            if (node.State is RefTo reference)
                return GetTypeLeafElement(reference.Node);
            if (node.State is ArrayOf array)
                return GetTypeLeafElement(array.ElementNode);
            return node;
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
        public static void BecomeReferenceFor(this SolvingNode referencedNode, SolvingNode original)
        {
            referencedNode = referencedNode.GetNonReference();
            original = original.GetNonReference();
            Merge(referencedNode, original);
        }



        /// <summary>
        /// Превращает неопределенное ограничение в ограничение с массивом
        /// </summary>
        /// <param name="descNodeName"></param>
        /// <param name="descendant"></param>
        /// <param name="ancestor"></param>
        /// <returns></returns>
        private static ArrayOf TransformToArrayOrNull(string descNodeName, SolvingConstrains descendant,
            ArrayOf ancestor)
        {
            if (descendant.NoConstrains)
            {
                var constrains = new SolvingConstrains();
                string eName;
                
                if (descNodeName.StartsWith("T") && descNodeName.Length > 1)
                    eName = "e" + descNodeName.Substring(1).ToLower() + "'";
                else
                    eName = "e" + descNodeName.ToLower() + "'";
                
                var node = new SolvingNode(eName, constrains, SolvingNodeType.TypeVariable);
                node.Ancestors.Add(ancestor.ElementNode);
                return new ArrayOf(node);
            }
            
            if (descendant.HasDescendant && descendant.Descedant is ArrayOf arrayEDesc)
            {
                if(arrayEDesc.Element is RefTo)
                {
                    var origin = arrayEDesc.ElementNode.GetNonReference();
                    if(origin.IsSolved)
                        return new ArrayOf(origin);
                }
                else if (arrayEDesc.ElementNode.IsSolved)
                {
                    return arrayEDesc;
                }
            }

            return null;
        }
    }
}