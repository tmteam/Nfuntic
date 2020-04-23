﻿using System.Collections.Generic;
using System.Linq;

namespace nfun.Ti4
{
    public class FinalizationResults
    {
        public FinalizationResults(SolvingNode[] typeVariables, SolvingNode[] namedNodes, SolvingNode[] syntaxNodes)
        {
            TypeVariables = typeVariables;
            NamedNodes = namedNodes;
            SyntaxNodes = syntaxNodes;
        }

        public SolvingNode GetVariableNode(string variableName) =>
            NamedNodes.First(n => n.Name == "T" + variableName);
        public IState GetVariable(string variableName) =>
            NamedNodes.First(n => n.Name == "T" + variableName).State;
        public SolvingNode GetSyntaxNode(int syntaxNode) =>
            SyntaxNodes.First(n => n.Name == syntaxNode.ToString());

        private IEnumerable<SolvingNode> AllNodes => TypeVariables.Union(NamedNodes).Union(SyntaxNodes);
        public IEnumerable<SolvingNode> Generics => AllNodes.Where(t => t?.State is Constrains);
        public int GenericsCount => AllNodes.Count(t => t?.State is Constrains);

        public SolvingNode[] TypeVariables { get; }
        public SolvingNode[] NamedNodes { get; }
        public SolvingNode[] SyntaxNodes { get; }

       
    }
}