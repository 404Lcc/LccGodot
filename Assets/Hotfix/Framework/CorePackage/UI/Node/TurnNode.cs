namespace LccGodot.Services.UI.Node;

public sealed class TurnNode
{
    public string NodeName { get; init; } = string.Empty;

    public NodeType NodeType { get; init; }

    public object[]? NodeArgs { get; init; }
}
