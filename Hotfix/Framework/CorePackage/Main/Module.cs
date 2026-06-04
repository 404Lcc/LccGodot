namespace LccHotfix
{
    public abstract class Module
    {
        internal virtual void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal virtual void LateUpdate()
        {
        }

        internal virtual void Shutdown()
        {
        }
    }
}
