using System.Collections.Generic;

namespace LccModel
{
    public static class StringTable
    {
        public static Dictionary<string, string> Strings { get; set; } = new();

        public static string Get(string key)
        {
            return Strings.TryGetValue(key, out var value) ? value : key;
        }

        public static string Get(string key, params object[] args)
        {
            return string.Format(Get(key), args);
        }
    }

    public sealed class Version
    {
        public int MinVersion { get; set; }
        public int MaxVersion { get; set; }
    }

    public sealed class VersionConfig
    {
        public List<string> PatchesAddresses { get; set; } = new();
    }

    public static class PatchConfig
    {
        public static Version? Version { get; set; }
        public static VersionConfig? VersionConfig { get; set; }
    }

    public enum AssetPlayMode
    {
        Offline,
        Host,
    }

    public static class AssetConfig
    {
        public const string DefaultPackageName = "DefaultPackage";
        public const string RawFilePackageName = "RawFilePackage";

        public static AssetPlayMode PlayMode { get; set; } = AssetPlayMode.Offline;

        public static readonly List<string> PackageList = new()
        {
            DefaultPackageName,
            RawFilePackageName,
        };
    }
}
