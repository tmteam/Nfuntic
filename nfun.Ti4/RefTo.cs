namespace nfun.Ti4
{
    public class RefTo: ISolvingState
    {
        public RefTo(SolvingNode node)
        {
            if (node.Type != SolvingNodeType.TypeVariable)
            {

            }
            Node = node;
        }

        public ISolvingState Element => Node.State; 
        public SolvingNode Node { get; }
        public override string ToString() => $"ref({Node.Name})";
    }
}