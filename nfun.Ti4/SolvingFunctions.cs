using System;
using System.Collections.Generic;
using System.Linq;

namespace nfun.Ti4
{
    public static class SolvingFunctions 
    {
        #region Merges

        public static IState GetMergedState(IState stateA, IState stateB)
        {
            if (stateB is Constrains c && c.NoConstrains)
                return stateA;

            if (stateA is IType typeA && typeA.IsSolved)
            {
                if (stateB is IType typeB && typeB.IsSolved)
                {
                    if (!typeB.Equals(typeA)) 
                        throw new InvalidOperationException();
                    return typeA;
                }
                if (stateB is Constrains constrainsB)
                {
                    if (!constrainsB.Fits(typeA))
                        throw new InvalidOperationException();
                    return typeA;
                }
            }
            if (stateA is Array arrayA)
            {
                if (stateB is Array arrayB)
                {
                    Merge(arrayA.ElementNode, arrayB.ElementNode);
                    return arrayA;
                }
            }
            if (stateA is Fun funA)
            {
                if (stateB is Fun funB)
                {
                    Merge(funA.ArgNode, funB.ArgNode);
                    Merge(funA.RetNode, funB.RetNode);
                    return funA;
                }
            }
            if (stateA is Constrains constrainsA)
            {
                if (stateB is Constrains constrainsB)
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
                if (node.State is Array array) 
                    HandleUpwardLimits(array.ElementNode);
                if (node.State is Fun fun)
                {
                    HandleUpwardLimits(fun.ArgNode);
                    HandleUpwardLimits(fun.RetNode);
                }
            }

            foreach (var node in toposortedNodes.Where(n=>!n.MemberOf.Any()))
                HandleUpwardLimits(node);
        }

        private static IState SetUpwardsLimits(SolvingNode descendant, SolvingNode ancestor)
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
                    case Primitive concreteAnc:
                    {
                        if (!typeDesc.CanBeImplicitlyConvertedTo(concreteAnc))
                            throw new InvalidOperationException();
                        return ancestor.State;
                    }
                    case Constrains constrainsAnc:
                    {
                        var result = constrainsAnc.GetCopy();
                        result.AddDescedant(typeDesc);
                        return result.GetOptimizedOrThrow();
                    }
                    case Array arrayAnc:
                    {
                        if (!(typeDesc is Array arrayDesc))
                            throw new NotSupportedException();
                        
                        descendant.Ancestors.Remove(ancestor);
                        arrayDesc.ElementNode.Ancestors.Add(arrayAnc.ElementNode);
                        return ancestor.State;
                    }
                    case Fun fun:
                    {
                        if(!(typeDesc is Fun funDesc))
                            throw new NotSupportedException();
                        descendant.Ancestors.Remove(ancestor);
                        funDesc.RetNode.Ancestors.Add(fun.RetNode);
                        funDesc.ArgNode.Ancestors.Add(fun.ArgNode);
                        return ancestor.State;
                    }
                    default:
                        throw new NotSupportedException();
                }
            }
           

            if (descendant.State is Constrains constrainsDesc)
            {
                switch (ancestor.State)
                {
                    case Primitive concreteAnc:
                        {
                            if (constrainsDesc.HasDescendant && constrainsDesc.Descedant?.CanBeImplicitlyConvertedTo(concreteAnc) != true)
                                throw new InvalidOperationException();
                            return ancestor.State;
                        }
                    case Constrains constrainsAnc:
                        {
                            var result = constrainsAnc.GetCopy();
                            result.AddDescedant(constrainsDesc.Descedant);
                            return result.GetOptimizedOrThrow();
                        }
                    case Array arrayAnc:
                    {
                        var result = TransformToArrayOrNull(descendant.Name, constrainsDesc, arrayAnc);
                        if(result==null)
                            throw new InvalidOperationException();
                        
                        result.ElementNode.Ancestors.Add(arrayAnc.ElementNode);
                        descendant.State = result;
                        descendant.Ancestors.Remove(ancestor);
                        return ancestor.State;
                    }
                    case Fun funAnc:
                    {
                        var result = TransformToFunOrNull(descendant.Name, constrainsDesc, funAnc);
                        if (result == null)
                            throw new InvalidOperationException();

                        result.RetNode.Ancestors.Add(funAnc.RetNode);
                        result.ArgNode.Ancestors.Add(funAnc.ArgNode);
                        descendant.State = result;
                        descendant.Ancestors.Remove(ancestor);
                        return ancestor.State;
                    }
                    default:
                        throw new NotSupportedException($"Ancestor type {ancestor.State.GetType().Name} is not supported");
                }
            }
            throw new NotSupportedException($"Descendant type {descendant.State.GetType().Name} is not supported");
        }
        #endregion

        #region Downward

        public static void SetDownwardsLimits(SolvingNode[] toposortedNodes)
        {
            void Downwards(SolvingNode descendant)
            {
                if (descendant.State is Array array)
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
        private static IState SetDownwardsLimits(SolvingNode descendant, SolvingNode ancestor)
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

            if (ancestor.State is Array ancArray)
            {
                if (descendant.State is Constrains constr)
                {
                    var result = TransformToArrayOrNull(descendant.Name, constr, ancArray);
                    if(result==null)
                        throw new InvalidOperationException();
                    descendant.State = result;
                }

                if (descendant.State is Array desArray)
                {
                    desArray.ElementNode.State = SetDownwardsLimits(desArray.ElementNode, ancArray.ElementNode);
                    return descendant.State;
                }
                throw new InvalidOperationException();
            }

            if (ancestor.State is Fun ancFun)
            {
                if (descendant.State is Constrains constr)
                {
                    var result = TransformToFunOrNull(descendant.Name, constr, ancFun);
                    if (result == null)
                        throw new InvalidOperationException();
                    descendant.State = result;
                }
                if (descendant.State is Fun desArray)
                {
                    desArray.ArgNode.State = SetDownwardsLimits(desArray.ArgNode, ancFun.ArgNode);
                    desArray.RetNode.State = SetDownwardsLimits(desArray.ArgNode, ancFun.RetNode);
                    return descendant.State;
                }
                throw new InvalidOperationException();
            }

            Primitive up = null;
            if (ancestor.State is Primitive concreteAnc) up = concreteAnc;
            else if (ancestor.State is Constrains constrainsAnc)
            {
                if (constrainsAnc.HasAncestor) up = constrainsAnc.Ancestor;
                else return descendant.State;
            }
            else if (ancestor.State is Array) return descendant.State;
            else if (ancestor.State is Fun) return descendant.State;

            if (up == null)
                throw new InvalidOperationException();

            if (descendant.State is IType concreteDesc)
            {
                if (concreteDesc.CanBeImplicitlyConvertedTo(up))
                    return descendant.State;
                throw new InvalidOperationException();
            }
            if (descendant.State is Constrains constrainsDesc)
            {
                constrainsDesc.AddAncestor(up);
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
                if (node.State is Array arrayDesc)
                {
                    Destruction(arrayDesc.ElementNode);
                    if (arrayDesc.Element is RefTo)
                       node.State = new Array(arrayDesc.ElementNode.GetNonReference());
                }

                if (node.State is Fun funDesc)
                {
                    Destruction(funDesc.ArgNode);
                    Destruction(funDesc.RetNode);

                    if (funDesc.ArgType is RefTo || funDesc.ReturnType is RefTo)
                        node.State = Fun.Of(funDesc.ArgNode.GetNonReference(), funDesc.RetNode.GetNonReference());
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
                case Primitive concreteAnc:
                {
                    if (nonRefDescendant.State is Constrains c && c.Fits(concreteAnc))
                    {
                        Console.Write("p+c");
                        nonRefDescendant.State = concreteAnc;
                    }
                    break;
                }

                case Array arrayAnc:
                {
                    if (nonRefDescendant.State is Constrains constrainsDesc && constrainsDesc.Fits(arrayAnc))
                    {
                        Console.Write("a+c");
                        nonRefDescendant.State = new RefTo(nonRefAncestor);
                    }
                    break;
                }
                case Fun funAnc:
                {
                    if (nonRefDescendant.State is Constrains constrainsDesc && constrainsDesc.Fits(funAnc))
                    {
                        Console.Write("f+c");
                        nonRefDescendant.State = new RefTo(nonRefAncestor);
                    }
                    break;
                }
                case Constrains constrainsAnc when nonRefDescendant.State is Primitive concreteDesc:
                {
                    if (constrainsAnc.Fits(concreteDesc))
                    {
                        Console.Write("c+p");
                        descendantNode.State = ancestorNode.State = nonRefAncestor.State = concreteDesc;
                    }

                    break;
                }
                case Constrains constrainsAnc when nonRefDescendant.State is Constrains constrainsDesc:
                {
                    Console.Write("c+c");

                    var result = constrainsAnc.MergeOrNull(constrainsDesc);
                    if (result == null)
                        return;

                    if (result is Primitive)
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

                case Constrains constrainsAnc when nonRefDescendant.State is Array arrayDes:
                {
                    Console.Write("c+a");

                    if (constrainsAnc.Fits(arrayDes))
                        nonRefAncestor.State = ancestorNode.State = new RefTo(nonRefDescendant);

                    break;
                }
                case Constrains constrainsAnc when nonRefDescendant.State is Fun funDes:
                {
                    Console.Write("c+f");

                    if (constrainsAnc.Fits(funDes))
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
                else if (node.State is Array array)
                {
                    if (array.Element is RefTo reference)
                    {
                        node.State = new Array(reference.Node.GetNonReference());
                        Console.WriteLine($"\t{node.Name}->ar");
                    }
                    Finalize((node.State as Array).ElementNode);
                }
            }

            foreach (var node in toposortedNodes)
            {
                Finalize(node);

                var concreteElement = node.GetTypeLeafElement();

                if (concreteElement.Type== SolvingNodeType.TypeVariable 
                    && concreteElement.State is Constrains)
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
            if (node.State is Array array)
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
        private static Array TransformToArrayOrNull(string descNodeName, Constrains descendant,
            Array ancestor)
        {
            if (descendant.NoConstrains)
            {
                var constrains = new Constrains();
                string eName;
                
                if (descNodeName.StartsWith("T") && descNodeName.Length > 1)
                    eName = "e" + descNodeName.Substring(1).ToLower() + "'";
                else
                    eName = "e" + descNodeName.ToLower() + "'";
                
                var node = new SolvingNode(eName, constrains, SolvingNodeType.TypeVariable);
                node.Ancestors.Add(ancestor.ElementNode);
                return new Array(node);
            }
            
            if (descendant.HasDescendant && descendant.Descedant is Array arrayEDesc)
            {
                if(arrayEDesc.Element is RefTo)
                {
                    var origin = arrayEDesc.ElementNode.GetNonReference();
                    if(origin.IsSolved)
                        return new Array(origin);
                }
                else if (arrayEDesc.ElementNode.IsSolved)
                {
                    return arrayEDesc;
                }
            }

            return null;
        }
        /// <summary>
        /// Превращает неопределенное ограничение в функциональный тип 
        /// </summary>
        private static Fun TransformToFunOrNull(string descNodeName, Constrains descendant, Fun ancestor)
        {
            if (descendant.NoConstrains)
            { 
                string eName;

                if (descNodeName.StartsWith("T") && descNodeName.Length > 1)
                    eName = "e" + descNodeName.Substring(1).ToLower() + "'";
                else
                    eName = "e" + descNodeName.ToLower() + "'";

                var argNode = new SolvingNode(eName, new Constrains(), SolvingNodeType.TypeVariable);
                argNode.Ancestors.Add(ancestor.ArgNode);
                var retNode = new SolvingNode(eName, new Constrains(), SolvingNodeType.TypeVariable);
                retNode.Ancestors.Add(ancestor.RetNode);

                return Fun.Of(argNode: argNode, returnNode: retNode);
            }

            if (descendant.HasDescendant && descendant.Descedant is Fun arrayEDesc)
            {
                if (arrayEDesc.IsSolved)
                    return arrayEDesc;

                var nrArgNode = arrayEDesc.ArgNode.GetNonReference();
                var nrRetNode = arrayEDesc.RetNode.GetNonReference();
                if (nrArgNode.IsSolved && nrRetNode.IsSolved)
                    return Fun.Of(nrRetNode, nrArgNode);
            }
            return null;
        }
    }
}