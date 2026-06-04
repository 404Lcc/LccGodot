namespace LccGodot.Services.UI;

public enum NodeType
{
    Element = 0,
    Domain = 1
}

public enum NodePhase
{
    Create = 0,
    Show = 1
}

public enum ReleaseType
{
    Auto = 0,
    Keep = 1
}

public enum EscapeType
{
    Skip = 0,
    Hide = 1
}

public enum UILayerId
{
    Hud = 0,
    Panel = 1,
    Popup = 2,
    Tips = 3,
    Guide = 4,
    Debug = 5
}
