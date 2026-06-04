using System;
using System.Collections.Generic;
using Godot;

namespace LccHotfix
{
    public enum TimerUnitType
    {
        Millisecond,
        Second,
        Minute,
        Hour,
        Day,
    }

    public sealed class TimerTask
    {
        private float _duration;
        private int _loopCount;
        private bool _ignoreTimeScale;
        private Action? _onComplete;
        private Action<float>? _onUpdate;
        private float _elapsed;
        private bool _isPaused;
        private bool _isDisposed;
        private bool _isDone;

        public bool IsCompleted => _isDone || _isDisposed;

        public void Register(float duration, TimerUnitType unitType, int loopCount, bool ignoreTimeScale, Action? onRegister, Action? onComplete, Action<float>? onUpdate)
        {
            _duration = ConvertUnitToSecond(duration, unitType);
            _loopCount = loopCount;
            _ignoreTimeScale = ignoreTimeScale;
            _onComplete = onComplete;
            _onUpdate = onUpdate;
            _elapsed = 0f;
            onRegister?.Invoke();
        }

        public void ResetDuration(float duration)
        {
            _duration = duration;
            _elapsed = 0f;
        }

        public void Pause()
        {
            if (!IsCompleted)
            {
                _isPaused = true;
            }
        }

        public void Resume()
        {
            _isPaused = false;
        }

        public void Dispose()
        {
            _isDisposed = true;
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (IsCompleted || _isPaused)
            {
                return;
            }

            _elapsed += _ignoreTimeScale ? realElapseSeconds : elapseSeconds;
            _onUpdate?.Invoke(GetLeftTime());

            if (_elapsed < _duration)
            {
                return;
            }

            try
            {
                _onComplete?.Invoke();
            }
            catch (Exception ex)
            {
                GD.PrintErr(ex.ToString());
            }

            if (_loopCount == -1)
            {
                _elapsed = 0f;
                return;
            }

            _loopCount--;
            if (_loopCount <= 0)
            {
                _isDone = true;
            }
            else
            {
                _elapsed = 0f;
            }
        }

        public float GetLeftTime()
        {
            return IsCompleted ? 0f : Math.Max(0f, _duration - _elapsed);
        }

        public float GetLeftTimeRatio()
        {
            return _duration <= 0f ? 1f : GetLeftTime() / _duration;
        }

        private static float ConvertUnitToSecond(float duration, TimerUnitType unitType)
        {
            return unitType switch
            {
                TimerUnitType.Millisecond => duration / 1000f,
                TimerUnitType.Minute => duration * 60f,
                TimerUnitType.Hour => duration * 3600f,
                TimerUnitType.Day => duration * 86400f,
                _ => duration,
            };
        }
    }

    internal sealed class TimerManager : Module, ITimerService
    {
        private readonly List<TimerTask> _tasks = new();
        private readonly List<TimerTask> _pendingTasks = new();

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            UpdateAll(elapseSeconds, realElapseSeconds);
        }

        internal override void Shutdown()
        {
            DisposeAll();
        }

        public TimerTask Register(float duration, TimerUnitType unitType = TimerUnitType.Second, int loopCount = 1, bool ignoreTimeScale = false, Action? onRegister = null, Action? onComplete = null, Action<float>? onUpdate = null)
        {
            var task = new TimerTask();
            task.Register(duration, unitType, loopCount, ignoreTimeScale, onRegister, onComplete, onUpdate);
            _pendingTasks.Add(task);
            return task;
        }

        public void DisposeAll()
        {
            foreach (var task in _tasks)
            {
                task.Dispose();
            }

            _pendingTasks.Clear();
            _tasks.Clear();
        }

        public void PauseAll()
        {
            foreach (var task in _tasks)
            {
                task.Pause();
            }
        }

        public void ResumeAll()
        {
            foreach (var task in _tasks)
            {
                task.Resume();
            }
        }

        public void UpdateAll(float elapseSeconds, float realElapseSeconds)
        {
            if (_pendingTasks.Count > 0)
            {
                _tasks.AddRange(_pendingTasks);
                _pendingTasks.Clear();
            }

            foreach (var task in _tasks)
            {
                task.Update(elapseSeconds, realElapseSeconds);
            }

            for (var i = _tasks.Count - 1; i >= 0; i--)
            {
                if (_tasks[i].IsCompleted)
                {
                    _tasks.RemoveAt(i);
                }
            }
        }
    }
}
