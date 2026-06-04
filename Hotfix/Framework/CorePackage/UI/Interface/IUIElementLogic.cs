namespace LccHotfix
{
    public interface IUIElementLogic : IUILogic
    {
        UILayerID LayerID { get; }
        bool IsFullScreen { get; }
        NodeType ReturnNodeType { get; }
        string ReturnNodeName { get; }
        int ReturnNodeParam { get; }
    }
}
