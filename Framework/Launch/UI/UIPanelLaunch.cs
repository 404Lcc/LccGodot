using System;
using System.Collections.Generic;

namespace LccModel.LaunchUI
{
    public static class UIPanelLaunch
    {
        public sealed class MessageBoxParams
        {
            public string Content { get; set; } = string.Empty;
            public List<MessageBoxOption> ButtonOptionList { get; set; } = new();
        }

        public sealed class MessageBoxOption
        {
            public string Name { get; set; } = string.Empty;
            public Action? Action { get; set; }
        }

        public static string FormatBytes(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB" };
            var size = (double)bytes;
            var unitIndex = 0;

            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            return $"{size:0.##}{units[unitIndex]}";
        }
    }
}
