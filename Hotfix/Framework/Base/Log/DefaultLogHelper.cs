using System;
using Godot;

namespace LccHotfix
{
    /// <summary>
    /// 默认游戏框架日志辅助器。
    /// </summary>
    public class DefaultLogHelper : ILogHelper
    {
        /// <summary>
        /// 记录日志。
        /// </summary>
        /// <param name="level">日志等级。</param>
        /// <param name="message">日志内容。</param>
        public void Log(LogLevel level, object message)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    GD.Print(string.Format("<color=#888888>{0}</color>", message.ToString()));
                    break;

                case LogLevel.Info:
                    GD.Print(message.ToString());
                    break;

                case LogLevel.Warning:
                    GD.PrintErr(message.ToString());
                    break;

                case LogLevel.Error:
                    GD.PrintErr(message.ToString());
                    break;

                default:
                    throw new Exception(message.ToString());
            }
        }
    }
}