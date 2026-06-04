using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using LccGodot.Core;

namespace LccGodot.Services.Asset;

public sealed class AssetService : Module, IAssetService
{
	private readonly Dictionary<string, Resource> _cache = new();
	private readonly Dictionary<string, HashSet<string>> _groups = new();

	internal override void Update(double delta, double realDelta)
	{
	}

	internal override void Shutdown()
	{
		_cache.Clear();
		_groups.Clear();
	}

	public T? Load<T>(string path) where T : Resource
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			GD.PushError("Asset path is empty.");
			return null;
		}

		if (_cache.TryGetValue(path, out Resource? cached))
		{
			return cached as T;
		}

		T? resource = GD.Load<T>(path);
		if (resource == null)
		{
			GD.PushError($"Asset load failed: {path}");
			return null;
		}

		_cache[path] = resource;
		AddToGroup("Default", path);
		return resource;
	}

	public async Task<T?> LoadAsync<T>(string path) where T : Resource
	{
		await Task.Yield();
		return Load<T>(path);
	}

	public async void LoadAsync<T>(string path, Action<T?> callback) where T : Resource
	{
		T? resource = await LoadAsync<T>(path);
		callback?.Invoke(resource);
	}

	public void Release(string path)
	{
		_cache.Remove(path);
		foreach (HashSet<string> paths in _groups.Values)
		{
			paths.Remove(path);
		}
	}

	public void ReleaseGroup(string group)
	{
		if (!_groups.TryGetValue(group, out HashSet<string>? paths))
		{
			return;
		}

		foreach (string path in paths)
		{
			_cache.Remove(path);
		}

		paths.Clear();
		_groups.Remove(group);
	}

	private void AddToGroup(string group, string path)
	{
		if (!_groups.TryGetValue(group, out HashSet<string>? paths))
		{
			paths = new HashSet<string>();
			_groups.Add(group, paths);
		}

		paths.Add(path);
	}
}
