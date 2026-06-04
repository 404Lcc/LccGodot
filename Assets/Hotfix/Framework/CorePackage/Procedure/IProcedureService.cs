using LccGodot.Core;

namespace LccGodot.Services.Procedure;

public interface IProcedureService : IService
{
    int CurState { get; }

    bool IsLoading { get; }

    LoadProcedureHandler? GetProcedure(int type);

    void ChangeProcedure(int type);

    void CleanProcedure();
}
