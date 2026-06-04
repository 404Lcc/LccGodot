using System;

namespace LccHotfix
{
    public interface ITimerService : IService
    {
        TimerTask Register(float duration, TimerUnitType unitType = TimerUnitType.Second, int loopCount = 1, bool ignoreTimeScale = false, Action? onRegister = null, Action? onComplete = null, Action<float>? onUpdate = null);
        void DisposeAll();
        void PauseAll();
        void ResumeAll();
        void UpdateAll(float elapseSeconds, float realElapseSeconds);
    }
}
