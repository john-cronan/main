namespace JPC.Common.Internal
{
    internal enum PathNodeType
    {
        Branch,
        Separator,
        VolumeName
    }

    internal class PathNode
    {
        private readonly PathNodeType _nodeType;
        private readonly string _text;

        public PathNode(PathNodeType nodeType, string text)
        {
            _nodeType = nodeType;
            _text = text;
        }

        public string Text => _text;
        internal PathNodeType NodeType => _nodeType;
    }
}
