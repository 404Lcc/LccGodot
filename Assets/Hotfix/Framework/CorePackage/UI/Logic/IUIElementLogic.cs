namespace LccGodot.Services.UI.Logic;

public interface IUIElementLogic : IUILogic
{
    UILayerId LayerId { get; }

    bool IsFullScreen { get; }

    NodeType ReturnNodeType { get; }

    string ReturnNodeName { get; }

    object[]? ReturnNodeArgs { get; }
}
