using System.Collections.Generic;
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
        public object GetVariable(string variableName) =>
            NamedNodes.First(n => n.Name == "T" + variableName).State;
        public object GetSyntaxNode(int syntaxNode) =>
            NamedNodes.First(n => n.Name == syntaxNode.ToString()).State;

        private IEnumerable<SolvingNode> AllNodes => TypeVariables.Union(NamedNodes).Union(SyntaxNodes);
        public IEnumerable<SolvingNode> Generics => AllNodes.Where(t => t?.State is SolvingConstrains);
        public int GenericsCount => AllNodes.Count(t => t?.State is SolvingConstrains);

        public SolvingNode[] TypeVariables { get; }
        public SolvingNode[] NamedNodes { get; }
        public SolvingNode[] SyntaxNodes { get; }
    }
}