namespace LccGodot.Services.Procedure;

public enum ProcedureType
{
    None = 0,
    Login = 1,
    Main = 2,
    Battle = 4
}

public static class ProcedureTypeExtensions
{
    public static int ToInt(this ProcedureType type)
    {
        return (int)type;
    }
}
