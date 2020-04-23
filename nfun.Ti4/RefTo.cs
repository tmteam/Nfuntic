namespace nfun.Ti4
{
    public class RefTo: IState
    {
        public RefTo(SolvingNode node)
        {
            if (node.Type != SolvingNodeType.TypeVariable)
            {

            }
            Node = node;
        }

        public IState Element => Node.State; 
        public SolvingNode Node { get; }
        public override string ToString() => $"ref({Node.Name})";
    }
}