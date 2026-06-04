namespace LccHotfix
{
    public interface IMainService : IService
    {
        void OnInstall();
        T AddModule<T>() where T : Module, IService, new();
    }
}
