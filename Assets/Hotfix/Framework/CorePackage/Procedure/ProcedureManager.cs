using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using LccGodot.Core;

namespace LccGodot.Services.Procedure;

public sealed class ProcedureManager : LccGodot.Core.Module, IProcedureService
{
    private readonly Dictionary<int, LoadProcedureHandler> _handlers = new();
    private LoadProcedureHandler? _currentHandler;
    private bool _inLoading;

    public ProcedureManager()
    {
        RegisterAttributedProcedures();
    }

    public int CurState => _currentHandler?.ProcedureType ?? 0;

    public bool IsLoading => _inLoading || _currentHandler == null || _currentHandler.IsLoading;

    internal override void Update(double delta, double realDelta)
    {
        if (_currentHandler == null || _currentHandler.IsLoading)
        {
            return;
        }

        _currentHandler.Tick();
    }

    internal override void LateUpdate()
    {
        if (_currentHandler == null || _currentHandler.IsLoading)
        {
            return;
        }

        _currentHandler.LateUpdate();
    }

    internal override void Shutdown()
    {
        CleanProcedure();
        _handlers.Clear();
        _inLoading = false;
    }

    public LoadProcedureHandler? GetProcedure(int type)
    {
        return _handlers.GetValueOrDefault(type);
    }

    public void ChangeProcedure(int type)
    {
        if (type == 0)
        {
            return;
        }

        if (!_handlers.TryGetValue(type, out LoadProcedureHandler? handler))
        {
            GD.PushError($"Procedure not found: {type}");
            return;
        }

        ChangeProcedure(handler);
    }

    public void CleanProcedure()
    {
        if (_currentHandler == null)
        {
            return;
        }

        _currentHandler.ProcedureExitHandler();
        _currentHandler.IsCleanup = true;
        _currentHandler = null;
    }

    private void ChangeProcedure(LoadProcedureHandler handler)
    {
        if (_currentHandler != null && _currentHandler.ProcedureType == handler.ProcedureType)
        {
            return;
        }

        if (!handler.ProcedureEnterStateHandler())
        {
            return;
        }

        LoadProcedureHandler? last = _currentHandler;
        _currentHandler = handler;
        _inLoading = false;

        handler.IsLoading = true;
        handler.IsCleanup = false;
        handler.ProcedureLoadHandler();

        if (last != null)
        {
            last.ProcedureExitHandler();
            last.IsCleanup = true;
        }

        GD.Print($"ChangeProcedure: {handler.ProcedureType} ({handler.GetType().Name})");
        handler.ProcedureStartHandler();
    }

    private void RegisterAttributedProcedures()
    {
        foreach (Type type in GetProcedureTypes())
        {
            LoadProcedureHandler handler = (LoadProcedureHandler)(Activator.CreateInstance(type)
                ?? throw new InvalidOperationException($"Can not create procedure '{type.FullName}'."));

            if (handler.ProcedureType == 0)
            {
                GD.PushError($"Procedure type can not be 0: {type.FullName}");
                continue;
            }

            _handlers[handler.ProcedureType] = handler;
        }
    }

    private static IEnumerable<Type> GetProcedureTypes()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(GetLoadableTypes)
            .Where(type =>
                !type.IsAbstract &&
                typeof(LoadProcedureHandler).IsAssignableFrom(type) &&
                type.GetCustomAttribute<ProcedureAttribute>() != null);
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type != null)!;
        }
    }
}
