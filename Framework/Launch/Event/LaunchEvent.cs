using System;
using LccModel.LaunchUI;

namespace LccModel
{
    public static class LaunchEvent
    {
        public sealed class StateChangedEventArgs : EventArgs
        {
            public string From { get; init; } = string.Empty;
            public string To { get; init; } = string.Empty;
        }

        public sealed class VersionEventArgs : EventArgs
        {
            public string VersionStr { get; init; } = string.Empty;
        }

        public sealed class MessageBoxEventArgs : EventArgs
        {
            public UIPanelLaunch.MessageBoxParams Params { get; init; } = new();
        }

        public sealed class ProgressEventArgs : EventArgs
        {
            public float Progress { get; init; } = 1f;
            public string ProgressText { get; init; } = string.Empty;
        }

        public static event Action<StateChangedEventArgs>? StateChanged;
        public static event Action<VersionEventArgs>? ShowVersion;
        public static event Action<MessageBoxEventArgs>? ShowMessageBox;
        public static event Action<ProgressEventArgs>? ShowProgress;

        public static void BroadcastStateChanged(string from, string to)
        {
            StateChanged?.Invoke(new StateChangedEventArgs { From = from, To = to });
        }

        public static void BroadcastShowVersion(string version)
        {
            ShowVersion?.Invoke(new VersionEventArgs { VersionStr = version });
        }

        public static void BroadcastShowMessageBox(UIPanelLaunch.MessageBoxParams parameters)
        {
            ShowMessageBox?.Invoke(new MessageBoxEventArgs { Params = parameters });
        }

        public static void BroadcastShowProgress(float progress, string progressText)
        {
            ShowProgress?.Invoke(new ProgressEventArgs
            {
                Progress = progress,
                ProgressText = progressText,
            });
        }
    }
}
