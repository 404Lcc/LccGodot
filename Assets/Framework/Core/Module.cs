namespace LccGodot.Core;

public abstract class Module
{
    internal abstract void Update(double delta, double realDelta);

    internal virtual void LateUpdate()
    {
    }

    internal abstract void Shutdown();
}
