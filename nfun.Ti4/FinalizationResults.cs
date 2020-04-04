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
        public SolvingNode[] TypeVariables { get; }
        public SolvingNode[] NamedNodes { get; }
        public SolvingNode[] SyntaxNodes { get; }
    }
}