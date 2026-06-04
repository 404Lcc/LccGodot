using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using LccGodot.Events;
using LccGodot.Services.Asset;
using LccGodot.Services.Procedure;
using LccGodot.Services.UI;

namespace LccGodot.Core;

public abstract class Main : Module
{
    private readonly LinkedList<Module> _modules = new();
    private readonly object _lock = new();

    public static Main? Current { get; private set; }

    public static IAssetService AssetService { get; protected set; } = null!;
    public static IProcedureService ProcedureService { get; protected set; } = null!;
    public static IUIService UIService { get; protected set; } = null!;

    internal override void Update(double delta, double realDelta)
    {
        lock (_lock)
        {
            foreach (Module module in _modules)
            {
                module.Update(delta, realDelta);
            }
        }
    }

    internal override void LateUpdate()
    {
        lock (_lock)
        {
            foreach (Module module in _modules)
            {
                module.LateUpdate();
            }
        }
    }

    internal override void Shutdown()
    {
        lock (_lock)
        {
            for (LinkedListNode<Module>? current = _modules.Last; current != null; current = current.Previous)
            {
                current.Value.Shutdown();
            }

            _modules.Clear();
            Event.ClearAll();
            Current = null;
        }
    }

    public T AddModule<T>() where T : Module, IService
    {
        lock (_lock)
        {
            Type moduleType = typeof(T);
            Module module = (Module)(Activator.CreateInstance(moduleType)
                ?? throw new InvalidOperationException($"Can not create module '{moduleType.FullName}'."));
            _modules.AddLast(module);
            return (T)module;
        }
    }

    public abstract void OnInstall();

    public abstract Task OnInitializeAsync();

    public static async Task SetMainAsync(Main main)
    {
        if (Current != null)
        {
            GD.PushError("SetMain failed: Current already exists.");
            return;
        }

        Current = main;
        main.OnInstall();
        await main.OnInitializeAsync();
    }
}
