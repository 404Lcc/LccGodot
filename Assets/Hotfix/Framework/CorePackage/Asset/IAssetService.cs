using System;
using System.Threading.Tasks;
using Godot;
using LccGodot.Core;

namespace LccGodot.Services.Asset;

public interface IAssetService : IService
{
    T? Load<T>(string path) where T : Resource;

    Task<T?> LoadAsync<T>(string path) where T : Resource;

    void LoadAsync<T>(string path, Action<T?> callback) where T : Resource;

    void Release(string path);

    void ReleaseGroup(string group);
}
